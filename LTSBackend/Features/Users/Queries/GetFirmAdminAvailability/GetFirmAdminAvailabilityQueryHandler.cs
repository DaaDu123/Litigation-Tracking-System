using LTSBackend.Comman.Enum;
using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using LTSBackend.Features.Users.DTOs;
using LTSBackend.Services.Availability;
using LTSBackend.Services.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.Users.Queries.GetFirmAdminAvailability;

public class GetFirmAdminAvailabilityQueryHandler(AppDbContext _context, ICurrentUserService _currentUser)
    : IRequestHandler<GetFirmAdminAvailabilityQuery, FirmAdminAvailabilityDTO>
{
    // =====================================================
    // HANDLE — the caller's own firm's FirmAdmin availability, evaluated
    // lazily through AvailabilityEvaluator so an expired window reads as
    // Active immediately, even before the background sweep runs.
    // The Users tenant query filter already restricts this to the
    // caller's own firm - no manual FirmID check needed.
    // =====================================================
    public async Task<FirmAdminAvailabilityDTO> Handle(GetFirmAdminAvailabilityQuery request, CancellationToken cancellationToken)
    {
        if (_currentUser.FirmID is null)
            throw new ValidationException(["You are not a member of a firm yet."]);

        var admin = await _context.Users.AsNoTracking()
            .FirstOrDefaultAsync(x => x.FirmID == _currentUser.FirmID && x.RoleID == (int)UserRole.FirmAdmin && !x.IsDeleted, cancellationToken)
            ?? throw new NotFoundException("Firm Admin not found for this firm.");

        var availability = AvailabilityEvaluator.GetEffective(admin);

        return new FirmAdminAvailabilityDTO
        {
            FirmAdminUserID = admin.UserID,
            FirmAdminName = admin.FullName,
            IsAvailable = availability.IsAvailable,
            Reason = availability.Reason,
            InactiveFromUtc = availability.InactiveFromUtc,
            InactiveUntilUtc = availability.InactiveUntilUtc
        };
    }
}
