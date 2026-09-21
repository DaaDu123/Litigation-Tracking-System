using LTSBackend.Comman.Exceptions;
using LTSBackend.Comman.Validation;
using LTSBackend.Data;
using LTSBackend.Features.Profile.DTOs;
using LTSBackend.Services.Audit;
using LTSBackend.Services.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.Profile.Commands.CompleteFirmAdminProfile;

public class CompleteFirmAdminProfileCommandHandler(AppDbContext _context, ICurrentUserService _currentUser, IAuditService _auditService,
    ILogger<CompleteFirmAdminProfileCommandHandler> _logger) : IRequestHandler<CompleteFirmAdminProfileCommand, ProfileCompletionResultDTO>
{
    // =====================================================
    // HANDLE — mandatory Firm Admin profile completion, run once
    // immediately after first login. Normalizes Phone/CNIC via
    // PakistaniFormat (same normalization used everywhere else, so
    // uniqueness can never be bypassed by reformatting), re-validates
    // uniqueness against every OTHER user, replaces the Firm's
    // placeholder name/contact details, and flips IsProfileCompleted.
    // This is the ONLY place IsProfileCompleted is set true for a Firm
    // Admin - not duplicated anywhere else.
    // =====================================================
    public async Task<ProfileCompletionResultDTO> Handle(CompleteFirmAdminProfileCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserID is null)
            throw new UnauthorizedException("You must be logged in.");

        var user = await _context.Users.Include(x => x.Firm).FirstOrDefaultAsync(x => x.UserID == _currentUser.UserID, cancellationToken)
            ?? throw new NotFoundException("User not found.");

        if (user.Firm == null)
            throw new ValidationException(["No firm workspace is associated with this account."]);

        var normalizedPhone = PakistaniFormat.NormalizePhone(request.Phone);
        var normalizedCnic = PakistaniFormat.NormalizeCnic(request.CNIC);
        var normalizedContactPhone = PakistaniFormat.NormalizePhone(request.ContactPhone);

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

        user.Firm.FirmName = request.FirmName.Trim();
        user.Firm.Address = request.Address;
        user.Firm.ContactEmail = request.ContactEmail;
        user.Firm.ContactPhone = normalizedContactPhone;

        _context.AuditLogs.Add(_auditService.Create(user.UserID, "Firm Admin completed profile and firm setup"));

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

        _logger.LogInformation("Firm Admin {UserId} completed profile", user.UserID);

        return new ProfileCompletionResultDTO
        {
            UserID = user.UserID,
            FullName = user.FullName,
            IsProfileCompleted = true,
            Message = "Profile completed. Welcome to your Firm Admin Dashboard."
        };
    }
}
