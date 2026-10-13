using LTSBackend.Data;
using LTSBackend.Features.Users.DTOs;
using LTSBackend.Features.Users.Queries.GetAllUsers;
using LTSBackend.Services.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

public class GetAllUsersQueryHandler(AppDbContext _context,ICurrentUserService _currentUser,
    ILogger<GetAllUsersQueryHandler> _logger) : IRequestHandler<GetAllUsersQuery, List<UserDTO>>
{







    public async Task<List<UserDTO>> Handle(GetAllUsersQuery request,CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching all users (active and deactivated - permanently deleted users are excluded)");

        var query = _context.Users.AsNoTracking().Where(x => !x.IsDeleted);



            query = query.Where(x => x.FirmID == _currentUser.FirmID);






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