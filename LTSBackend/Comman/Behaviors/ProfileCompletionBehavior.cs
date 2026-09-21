using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using LTSBackend.Services.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Comman.Behaviors;

/// <summary>
/// Enforces "Firm Admin / Firm User must complete their profile before
/// accessing anything else" at the backend, not just by hiding UI:
///   - Runs for EVERY MediatR request (command or query).
///   - Skipped entirely for anonymous/unauthenticated callers (login,
///     registration, forgot-password, etc. are naturally unauthenticated
///     and have no CurrentUser.UserID).
///   - For an authenticated caller whose profile is incomplete, ONLY
///     requests implementing IAllowIncompleteProfile (login-adjacent
///     commands and the profile-completion commands/queries themselves)
///     are allowed through; everything else is rejected with a clear,
///     non-technical validation message.
/// This is the single place this rule is enforced - no duplicate checks
/// scattered across individual handlers.
/// </summary>
public class ProfileCompletionBehavior<TRequest, TResponse>(AppDbContext _context, ICurrentUserService _currentUser)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request is IAllowIncompleteProfile)
            return await next();

        if (!_currentUser.IsAuthenticated || _currentUser.UserID is null)
            return await next();

        // SuperAdmin has no firm-scoped profile-completion requirement.
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
