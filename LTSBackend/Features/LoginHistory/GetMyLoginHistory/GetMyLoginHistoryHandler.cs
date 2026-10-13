using LTSBackend.Data;
using LTSBackend.Features.LoginHistory.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.LoginHistory.GetMyLoginHistory;








public class GetMyLoginHistoryHandler(AppDbContext context) : IRequestHandler<GetMyLoginHistoryQuery, List<MyLoginHistoryDTO>>
{




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