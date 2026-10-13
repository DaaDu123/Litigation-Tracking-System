using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using LTSBackend.Services.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Comman.Behaviors;
















public class ProfileCompletionBehavior<TRequest, TResponse>(AppDbContext _context, ICurrentUserService _currentUser)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request is IAllowIncompleteProfile)
            return await next();

        if (!_currentUser.IsAuthenticated || _currentUser.UserID is null)
            return await next();


        if (_currentUser.IsSuperAdmin)
            return await next();

        var isProfileCompleted = await _context.Users
            .AsNoTracking()
            .Where(x => x.UserID == _currentUser.UserID)
            .Select(x => x.IsProfileCompleted)
            .FirstOrDefaultAsync(cancellationToken);

        if (!isProfileCompleted)
            throw new ValidationException(["Please complete your profile before continuing."]);

        return await next();
    }
}
