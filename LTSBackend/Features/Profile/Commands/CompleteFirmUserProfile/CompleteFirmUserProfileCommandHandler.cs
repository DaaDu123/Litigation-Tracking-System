using LTSBackend.Comman.Exceptions;
using LTSBackend.Comman.Validation;
using LTSBackend.Data;
using LTSBackend.Features.Profile.DTOs;
using LTSBackend.Services.Audit;
using LTSBackend.Services.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.Profile.Commands.CompleteFirmUserProfile;

public class CompleteFirmUserProfileCommandHandler(AppDbContext _context, ICurrentUserService _currentUser, IAuditService _auditService,
    ILogger<CompleteFirmUserProfileCommandHandler> _logger) : IRequestHandler<CompleteFirmUserProfileCommand, ProfileCompletionResultDTO>
{
    // =====================================================
    // HANDLE — mandatory Firm User profile completion, required before
    // the "browse Firm Admins / send a request" functionality (enforced
    // by ProfileCompletionBehavior, not duplicated here). Same
    // normalize-then-uniqueness-check pattern as
    // CompleteFirmAdminProfileCommandHandler for Phone/CNIC.
    // =====================================================
    public async Task<ProfileCompletionResultDTO> Handle(CompleteFirmUserProfileCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserID is null)
            throw new UnauthorizedException("You must be logged in.");

        var user = await _context.Users.FirstOrDefaultAsync(x => x.UserID == _currentUser.UserID, cancellationToken)
            ?? throw new NotFoundException("User not found.");

        var normalizedPhone = PakistaniFormat.NormalizePhone(request.Phone);
        var normalizedCnic = PakistaniFormat.NormalizeCnic(request.CNIC);

        bool phoneTaken = await _context.Users.AsNoTracking().AnyAsync(x => x.UserID != user.UserID && x.Phone == normalizedPhone && !x.IsDeleted, cancellationToken);
        if (phoneTaken)
            throw new ValidationException(["Contact number already exists."]);

        bool cnicTaken = await _context.Users.AsNoTracking().AnyAsync(x => x.UserID != user.UserID && x.CNIC == normalizedCnic && !x.IsDeleted, cancellationToken);
        if (cnicTaken)
            throw new ValidationException(["CNIC already exists."]);

        user.FullName = request.FullName.Trim();
        user.Phone = normalizedPhone;
        user.CNIC = normalizedCnic;
        user.IsProfileCompleted = true;

        _context.AuditLogs.Add(_auditService.Create(user.UserID, "Firm User completed profile"));

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("IX_Users_Phone", StringComparison.OrdinalIgnoreCase) == true)
        {
            _logger.LogWarning(ex, "Concurrent phone-uniqueness race for user {UserId}", user.UserID);
            throw new ValidationException(["Contact number already exists."]);
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("IX_Users_CNIC", StringComparison.OrdinalIgnoreCase) == true)
        {
            _logger.LogWarning(ex, "Concurrent CNIC-uniqueness race for user {UserId}", user.UserID);
            throw new ValidationException(["CNIC already exists."]);
        }

        _logger.LogInformation("Firm User {UserId} completed profile", user.UserID);

        return new ProfileCompletionResultDTO
        {
            UserID = user.UserID,
            FullName = user.FullName,
            IsProfileCompleted = true,
            Message = "Profile completed. You can now browse and request a firm to join."
        };
    }
}
