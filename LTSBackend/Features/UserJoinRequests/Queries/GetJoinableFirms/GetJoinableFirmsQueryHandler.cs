using LTSBackend.Data;
using LTSBackend.Features.UserJoinRequests.DTOs;
using LTSBackend.Models.Security;
using LTSBackend.Services.Availability;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.UserJoinRequests.Queries.GetJoinableFirms;

public class GetJoinableFirmsQueryHandler(AppDbContext _context) : IRequestHandler<GetJoinableFirmsQuery, List<JoinableFirmDTO>>
{
    // =====================================================
    // HANDLE — "Available Firm Admins" directory for the Firm User
    // dashboard. Requires [Authorize] on the controller (any authenticated
    // user with a completed profile and no firm yet - enforced by
    // ProfileCompletionBehavior + SubmitUserJoinRequestCommandHandler),
    // and deliberately returns only public-facing fields - no email, no
    // CNIC, no internal identifiers beyond FirmID. Availability is
    // evaluated through AvailabilityEvaluator so an expired inactive
    // window shows as "Available" immediately, even if the background
    // reactivation sweep hasn't run yet.
    // =====================================================
    public async Task<List<JoinableFirmDTO>> Handle(GetJoinableFirmsQuery request, CancellationToken cancellationToken)
    {
        var firms = await _context.Firms.AsNoTracking()
            .Where(x => !x.IsDeleted && !x.IsBlocked)
            .OrderBy(x => x.FirmName)
            .ToListAsync(cancellationToken);

        var firmIds = firms.Select(f => f.FirmID).ToList();

        // One Firm Admin per firm expected; if a firm somehow has more
        // than one active FirmAdmin row, the first (by UserID) represents it.
        var admins = await _context.Users.AsNoTracking().IgnoreQueryFilters()
            .Where(x => x.FirmID != null && firmIds.Contains(x.FirmID.Value)
                     && x.RoleID == (int)Comman.Enum.UserRole.FirmAdmin
                     && x.IsActive && !x.IsDeleted && x.IsProfileCompleted)
            .ToListAsync(cancellationToken);

        var adminByFirm = admins.GroupBy(a => a.FirmID!.Value).ToDictionary(g => g.Key, g => g.First());

        var result = new List<JoinableFirmDTO>();

        foreach (var firm in firms)
        {
            adminByFirm.TryGetValue(firm.FirmID, out var admin);

            var availability = admin != null ? AvailabilityEvaluator.GetEffective(admin) : new EffectiveAvailability(true, null, null, null);

            result.Add(new JoinableFirmDTO
            {
                FirmID = firm.FirmID,
                FirmName = firm.FirmName,
                FirmAdminName = admin?.FullName,
                FirmAdminContactNumber = admin?.Phone,
                IsFirmAdminAvailable = availability.IsAvailable,
                FirmAdminAvailableAgainAtUtc = availability.InactiveUntilUtc
            });
        }

        return result;
    }
}
