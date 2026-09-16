using LTSBackend.Data;
using LTSBackend.Features.Users.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.Users.Queries.GetDeletedUsers;

public class GetDeletedUsersQueryHandler(AppDbContext _context, ILogger<GetDeletedUsersQueryHandler> _logger) : IRequestHandler<GetDeletedUsersQuery, List<DeletedUserDTO>>
{
    // =====================================================
    // HANDLE — SuperAdmin-only list of every soft-deleted user, all firms
    // Email-reuse candidates: shows each deleted record's original firm
    // and whether it's already been released for cross-firm reuse (see
    // ReleaseUserEmailCommand). Uses IgnoreQueryFilters() deliberately —
    // this view exists specifically to see across every firm's deleted
    // records.
    // =====================================================
    public async Task<List<DeletedUserDTO>> Handle(GetDeletedUsersQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching deleted users (email-reuse candidates) across all firms");

        var users = await _context.Users
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.IsDeleted)
            .Include(x => x.Role)
            .Include(x => x.Firm)
            .OrderByDescending(x => x.UpdatedAt)
            .Select(x => new DeletedUserDTO
            {
                UserID = x.UserID,
                FullName = x.FullName,
                Email = x.Email,
                FirmID = x.FirmID,
                FirmName = x.Firm != null ? x.Firm.FirmName : null,
                RoleName = x.Role != null ? x.Role.RoleName : null,
                IsReleasedForReuse = x.IsReleasedForReuse,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Retrieved {Count} deleted user records", users.Count);

        return users;
    }
}
