using LTSBackend.Comman.Enum;
using LTSBackend.Data;
using LTSBackend.Features.UserJoinRequests.DTOs;
using LTSBackend.Services.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.UserJoinRequests.Queries.GetMyJoinRequest;

public class GetMyJoinRequestQueryHandler(AppDbContext _context, ICurrentUserService _currentUser)
    : IRequestHandler<GetMyJoinRequestQuery, UserJoinRequestDTO?>
{







    public async Task<UserJoinRequestDTO?> Handle(GetMyJoinRequestQuery request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserID is null)
            return null;

        var joinRequest = await _context.UserJoinRequests
            .IgnoreQueryFilters()
            .Include(x => x.Firm)
            .Where(x => x.UserID == _currentUser.UserID)
            .OrderByDescending(x => x.RequestedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (joinRequest == null)
            return null;

        return new UserJoinRequestDTO
        {
            RequestID = joinRequest.RequestID,
            FirmID = joinRequest.FirmID,
            FirmName = joinRequest.Firm?.FirmName,
            UserID = joinRequest.UserID,
            FullName = joinRequest.FullName,
            Email = joinRequest.Email,
            Status = joinRequest.Status,
            RequestedAt = joinRequest.RequestedAt,
            ReviewedAt = joinRequest.ReviewedAt,
            RejectionReason = joinRequest.RejectionReason,
            CreatedUserID = joinRequest.CreatedUserID
        };
    }
}
