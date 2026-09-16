using LTSBackend.Data;
using LTSBackend.Features.LoginHistory.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.LoginHistory.GetMyLoginHistory;

/// <summary>
/// Returns the acting user's own login history only. UserID here is always
/// the caller's own ID (read from their JWT NameIdentifier claim in
/// LoginHistoryController.MyHistory(), never taken from client-supplied
/// input), so there is no IDOR risk on this endpoint even though the query
/// below filters by a raw UserID parameter.
/// </summary>
public class GetMyLoginHistoryHandler(AppDbContext context) : IRequestHandler<GetMyLoginHistoryQuery, List<MyLoginHistoryDTO>>
{
    // =====================================================
    // HANDLE — fetches every login record for the given (always self) user
    // Newest first.
    // =====================================================
    public async Task<List<MyLoginHistoryDTO>> Handle(GetMyLoginHistoryQuery request, CancellationToken cancellationToken)
    {
        return await context.LoginHistories
            .AsNoTracking()
            .Where(x => x.UserID == request.UserID)
            .OrderByDescending(x => x.LoginTime)
            .Select(x => new MyLoginHistoryDTO
            {
                LoginID = x.LoginID,
                LoginTime = x.LoginTime,
                LogoutTime = x.LogoutTime,
                IPAddress = x.IPAddress,
                UserAgent = x.UserAgent,
                Status = x.Status,
                IsLoggedOut = x.IsLoggedOut
            })
            .ToListAsync(cancellationToken);
    }
}