using LTSBackend.Comman.Enum;
using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using LTSBackend.Services.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Comman.Behaviors;















public class AccountStatusGuardBehavior<TRequest, TResponse>(AppDbContext _context, ICurrentUserService _currentUser)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserID is null || _currentUser.IsSuperAdmin)
            return await next();

        var status = await _context.Users
            .AsNoTracking()
            .Where(x => x.UserID == _currentUser.UserID)
            .Select(x => new
            {
                x.IsDeleted,
                x.IsActive,
                x.MembershipStatus,
                FirmIsBlocked = x.Firm != null && x.Firm.IsBlocked,
                FirmIsDeleted = x.Firm != null && x.Firm.IsDeleted
            })
            .FirstOrDefaultAsync(cancellationToken);


        if (status == null || status.IsDeleted)
            throw new UnauthorizedException("Your account is no longer active.");

        if (!status.IsActive)
            throw new UnauthorizedException("Your account is currently inactive.");

        if (status.MembershipStatus == MembershipStatuses.Blocked)
            throw new UnauthorizedException("Your access to this firm has been blocked. Please contact your Firm Admin.");

        if (status.MembershipStatus == MembershipStatuses.Removed)
            throw new UnauthorizedException("You are no longer a member of this firm.");

        if (status.FirmIsDeleted)
            throw new UnauthorizedException("This firm workspace is no longer active.");

        if (status.FirmIsBlocked)
            throw new UnauthorizedException("Your firm's workspace is currently blocked. Please contact your administrator.");

        return await next();
    }
}
