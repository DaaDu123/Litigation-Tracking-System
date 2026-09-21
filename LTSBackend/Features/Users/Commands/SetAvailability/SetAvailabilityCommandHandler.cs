using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using LTSBackend.Models.Security;
using LTSBackend.Services.Audit;
using LTSBackend.Services.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.Users.Commands.SetAvailability;

public class SetAvailabilityCommandHandler(AppDbContext _context, ICurrentUserService _currentUser, IAuditService _auditService,
    ILogger<SetAvailabilityCommandHandler> _logger) : IRequestHandler<SetAvailabilityCommand, bool>
{
    // =====================================================
    // HANDLE — Firm Admin sets their own Active/Inactive availability.
    // Stores an absolute InactiveUntilUtc timestamp (computed server-side
    // from UtcNow + the given Days/Hours/Minutes) rather than relying on
    // the raw duration fields at read time, per spec. Every downstream
    // read (GetFirmAdminAvailabilityQuery, the Firm Users directory,
    // GetJoinableFirms) goes through AvailabilityEvaluator, so an expired
    // window is treated as Active immediately even before the background
    // sweep (FirmAdminAvailabilityReactivationService) runs.
    // =====================================================
    public async Task<bool> Handle(SetAvailabilityCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserID is null)
            throw new UnauthorizedException("You must be logged in.");

        var user = await _context.Users.FirstOrDefaultAsync(x => x.UserID == _currentUser.UserID, cancellationToken)
            ?? throw new NotFoundException("User not found.");

        if (request.IsAvailable)
        {
            user.IsAvailable = true;
            user.InactiveReason = null;
            user.InactiveFromUtc = null;
            user.InactiveUntilUtc = null;

            _context.AuditLogs.Add(_auditService.Create(user.UserID, "Set availability: Active"));
        }
        else
        {
            var duration = new TimeSpan(request.Days, request.Hours, request.Minutes, 0);

            if (duration <= TimeSpan.Zero)
                throw new ValidationException(["Inactive duration is required and must be greater than zero."]);

            var now = DateTime.UtcNow;

            user.IsAvailable = false;
            user.InactiveReason = request.Reason;
            user.InactiveFromUtc = now;
            user.InactiveUntilUtc = now.Add(duration);

            _context.AuditLogs.Add(_auditService.Create(user.UserID, $"Set availability: Inactive until {user.InactiveUntilUtc:u} ({request.Reason})"));
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User {UserId} set availability to {IsAvailable}", user.UserID, request.IsAvailable);

        return true;
    }
}
