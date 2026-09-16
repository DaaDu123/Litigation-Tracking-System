using LTSBackend.Comman.Exceptions;
using LTSBackend.Comman.Middleware;
using LTSBackend.Data;
using LTSBackend.Features.Profile.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.Profile.Queries;

public class GetMyProfileHandler(AppDbContext _context, ILogger<GetMyProfileHandler> _logger) : IRequestHandler<GetMyProfileQuery, ProfileDTO>
{

    // =====================================================
    // HANDLE — fetches the caller's own profile
    // request.UserID is always the caller's own ID (from their JWT
    // claim), so this can never return another user's profile.
    // =====================================================
    public async Task<ProfileDTO> Handle(GetMyProfileQuery request,CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching profile for user: {UserId}", request.UserID);

        var user = await _context.Users
            .AsNoTracking()
            .Include(x => x.Role)
            .FirstOrDefaultAsync(x => x.UserID == request.UserID,cancellationToken);

        if (user == null)
        {
            _logger.LogWarning("Profile fetch failed: User not found: {UserId}", request.UserID);
            throw new NotFoundException("User not found.");
        }

        var profile = new ProfileDTO
        {
            UserID = user.UserID,
            FullName = user.FullName,
            Email = user.Email,
            Phone = user.Phone,
            Department = user.Department,
            ProfileImage = user.ProfileImage,
            RoleName = user.Role?.RoleName
        };

        _logger.LogInformation("Profile fetched for user: {UserId}", request.UserID);

        return profile;
    }
}