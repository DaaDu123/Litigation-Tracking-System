using LTSBackend.Comman.Enum;
using LTSBackend.Data;
using LTSBackend.Features.Users.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.Users.Queries.GetBlockedFirmUsers;

public class GetBlockedFirmUsersQueryHandler(AppDbContext _context) : IRequestHandler<GetBlockedFirmUsersQuery, List<BlockedFirmUserDTO>>
{
    // =====================================================
    // HANDLE — lists Blocked users in the caller's own firm. Firm-scoped
    // automatically via the Users tenant query filter. Blocked users keep
    // their FirmID (only Removed clears it), which is exactly what makes
    // them visible here for the Firm Admin to review/unblock.
    // =====================================================
    public async Task<List<BlockedFirmUserDTO>> Handle(GetBlockedFirmUsersQuery request, CancellationToken cancellationToken)
    {
        var blocked = await _context.Users.AsNoTracking()
            .Where(x => x.MembershipStatus == MembershipStatuses.Blocked)
            .Select(x => new BlockedFirmUserDTO
            {
                UserID = x.UserID,
                FullName = x.FullName,
                Email = x.Email,
                Phone = x.Phone,
                BlockedAtUtc = x.MembershipBlockedAtUtc,
                BlockedReason = x.MembershipBlockedReason,
            })
            .OrderByDescending(x => x.BlockedAtUtc)
            .ToListAsync(cancellationToken);

        var blockedByIds = await _context.Users.AsNoTracking()
            .Where(x => x.MembershipStatus == MembershipStatuses.Blocked && x.MembershipBlockedByUserID != null)
            .Select(x => x.MembershipBlockedByUserID!.Value)
            .Distinct()
            .ToListAsync(cancellationToken);

        if (blockedByIds.Count > 0)
        {
            var names = await _context.Users.AsNoTracking().IgnoreQueryFilters()
                .Where(x => blockedByIds.Contains(x.UserID))
                .ToDictionaryAsync(x => x.UserID, x => x.FullName, cancellationToken);

            var fullRows = await _context.Users.AsNoTracking()
                .Where(x => x.MembershipStatus == MembershipStatuses.Blocked)
                .Select(x => new { x.UserID, x.MembershipBlockedByUserID })
                .ToListAsync(cancellationToken);

            var byUserId = fullRows.ToDictionary(x => x.UserID, x => x.MembershipBlockedByUserID);

            foreach (var dto in blocked)
            {
                if (byUserId.TryGetValue(dto.UserID, out var blockedBy) && blockedBy.HasValue && names.TryGetValue(blockedBy.Value, out var name))
                    dto.BlockedByName = name;
            }
        }

        return blocked;
    }
}
