using LTSBackend.Data;
using LTSBackend.Features.Users.DTOs;
using LTSBackend.Features.Users.Queries.GetAllUsers;
using LTSBackend.Services.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

public class GetAllUsersQueryHandler(AppDbContext _context,ICurrentUserService _currentUser,
    ILogger<GetAllUsersQueryHandler> _logger) : IRequestHandler<GetAllUsersQuery, List<UserDTO>>
{   
    // =====================================================
    // HANDLE — lists active + deactivated users within the caller's own firm
    // Firm-scoped (SuperAdmin can't reach this endpoint at all — user
    // directory is FirmAdmin's job, per route-level [Authorize]).
    // Permanently deleted (IsDeleted) users are excluded — see
    // GetDeletedUsersQueryHandler for those.
    // =====================================================
    public async Task<List<UserDTO>> Handle(GetAllUsersQuery request,CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching all users (active and deactivated - permanently deleted users are excluded)");

        var query = _context.Users.AsNoTracking().Where(x => !x.IsDeleted);

        // Multi-tenancy: firm-scoped. SuperAdmin cannot reach this endpoint at all
        // (route-level [Authorize] excludes it - user directory is FirmAdmin's job).
            query = query.Where(x => x.FirmID == _currentUser.FirmID);

        // Server-side search by Name or Contact Number - never loads the
        // full firm directory client-side just to filter it there.
        // Phone is normalized at write time (PakistaniFormat), so a
        // partial digit search still matches regardless of how the
        // caller typed it, as long as the digits themselves match.
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim();
            query = query.Where(x => EF.Functions.Like(x.FullName, $"%{term}%") || (x.Phone != null && EF.Functions.Like(x.Phone, $"%{term}%")));
        }

        var users = await query
            .Include(x => x.Role)
            .OrderBy(x => x.FullName)
            .Select(x => new UserDTO
            {
                UserID = x.UserID,
                FullName = x.FullName,
                Email = x.Email,
                ProfileImage = x.ProfileImage,
                Phone = x.Phone,
                Department = x.Department,
                RoleID = x.RoleID,
                RoleName = x.Role != null ? x.Role.RoleName : null,
                IsActive = x.IsActive,
                IsLockedOut = x.LockoutEndUtc != null && x.LockoutEndUtc > DateTime.UtcNow,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt,
                MembershipStatus = x.MembershipStatus,
                IsAvailable = x.IsAvailable,
                InactiveUntilUtc = x.InactiveUntilUtc
            })
            .ToListAsync(cancellationToken);

        // Availability is evaluated per-row through the same lazy-healing
        // rule as everywhere else, rather than trusting the raw
        // IsAvailable/InactiveUntilUtc columns directly - a FirmAdmin row
        // whose window has already expired should read as Available here too.
        foreach (var dto in users.Where(u => !u.IsAvailable))
        {
            if (dto.InactiveUntilUtc.HasValue && dto.InactiveUntilUtc.Value <= DateTime.UtcNow)
            {
                dto.IsAvailable = true;
                dto.InactiveUntilUtc = null;
            }
        }

        _logger.LogInformation("Retrieved {Count} users (search: {Search})", users.Count, request.SearchTerm ?? "(none)");

        return users;
    }
}