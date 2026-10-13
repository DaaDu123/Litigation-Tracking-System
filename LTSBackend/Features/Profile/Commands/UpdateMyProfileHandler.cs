using LTSBackend.Comman.Exceptions;
using LTSBackend.Comman.Middleware;
using LTSBackend.Data;
using LTSBackend.Services.ProfileService;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.Profile.Commands;

public class UpdateMyProfileHandler(AppDbContext _context, IFileService _fileService, ILogger<UpdateMyProfileHandler> _logger) : IRequestHandler<UpdateMyProfileCommand, bool>
{








    public async Task<bool> Handle(UpdateMyProfileCommand request,CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating profile for user: {UserId}", request.UserID);


        var user = await _context.Users.FirstOrDefaultAsync(x => x.UserID == request.UserID, cancellationToken);

        if (user == null)
        {
            _logger.LogWarning("Profile update failed: User not found: {UserId}", request.UserID);
            throw new NotFoundException("User not found.");
        }


        if (request.ProfileImage != null)
        {

            if (!string.IsNullOrEmpty(user.ProfileImage))
            {
                _fileService.DeleteFile(user.ProfileImage);
                _logger.LogInformation("Old profile image deleted for user: {UserId}", request.UserID);
            }


            user.ProfileImage = await _fileService.SaveFileAsync(request.ProfileImage, "profile_pictures");
            _logger.LogInformation("New profile image saved for user: {UserId}", request.UserID);
        }


        user.FullName = request.FullName?.Trim() ?? string.Empty;
        user.Phone = string.IsNullOrWhiteSpace(request.Phone) ? request.Phone : request.Phone.Trim();
        user.Department = string.IsNullOrWhiteSpace(request.Department) ? request.Department : request.Department.Trim();
        user.UpdatedAt = DateTime.UtcNow;


        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Profile updated successfully for user: {UserId}", request.UserID);

        return true;
    }
}