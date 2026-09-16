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
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Retrieved {Count} active users", users.Count);

        return users;
    }
}