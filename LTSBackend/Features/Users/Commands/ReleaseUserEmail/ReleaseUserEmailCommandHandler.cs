using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.Users.Commands.ReleaseUserEmail;

public class ReleaseUserEmailCommandHandler(AppDbContext _context, ILogger<ReleaseUserEmailCommandHandler> _logger) : IRequestHandler<ReleaseUserEmailCommand, bool>
{








    public async Task<bool> Handle(ReleaseUserEmailCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Release-for-reuse request for user: {UserId}", request.UserID);





        var user = await _context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.UserID == request.UserID, cancellationToken);

        if (user == null)
        {
            _logger.LogWarning("Release failed: User not found: {UserId}", request.UserID);
            throw new NotFoundException("User not found.");
        }




        if (!user.IsDeleted)
        {
            _logger.LogWarning("Release failed: User is not deleted: {UserId}", request.UserID);
            throw new ValidationException(["Only a deleted user's email can be released for reassignment."]);
        }

        if (user.IsReleasedForReuse)
        {
            _logger.LogWarning("Release failed: Email already released: {UserId}", request.UserID);
            throw new ValidationException(["This email has already been released for reassignment."]);
        }






        user.IsReleasedForReuse = true;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Email released for reassignment for user: {UserId}", request.UserID);

        return true;
    }
}
