using LTSBackend.Comman.Exceptions;
using LTSBackend.Comman.Middleware;
using LTSBackend.Data;
using LTSBackend.Features.Users.DTOs;
using LTSBackend.Features.Users.Queries.GetUserById;
using LTSBackend.Services.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

public class GetUserByIdQueryHandler(AppDbContext _context, ICurrentUserService _currentUser, ILogger<GetUserByIdQueryHandler> _logger) : IRequestHandler<GetUserByIdQuery, UserDTO?>
{

    // =====================================================
    // HANDLE — fetches a single active user's profile by ID
    // Firm-scoped (can't fetch a user outside your own firm), and only
    // ever returns an active, non-deleted user — 404 otherwise, which
    // also backs UsersController.GetById / GetMyProfile.
    // =====================================================
    public async Task<UserDTO?> Handle(GetUserByIdQuery request,CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching user: {UserId}", request.UserID);

        var query = _context.Users
            .AsNoTracking()
            .Where(x => x.IsActive && !x.IsDeleted && x.UserID == request.UserID);

        // Multi-tenancy: can't fetch a user outside your own firm.
            query = query.Where(x => x.FirmID == _currentUser.FirmID);

        var user = await query
            .Include(x => x.Role)
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
            .FirstOrDefaultAsync(cancellationToken);

        if (user == null)
        {
            _logger.LogWarning("User not found or is inactive: {UserId}", request.UserID);
            throw new NotFoundException($"User with ID {request.UserID} not found.");
        }

        _logger.LogInformation("User retrieved: {UserId}", request.UserID);

        return user;
    }
}