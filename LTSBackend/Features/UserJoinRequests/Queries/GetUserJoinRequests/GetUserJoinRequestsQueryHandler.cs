using LTSBackend.Comman.Enum;
using LTSBackend.Data;
using LTSBackend.Features.UserJoinRequests.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.UserJoinRequests.Queries.GetUserJoinRequests;

public class GetUserJoinRequestsQueryHandler(AppDbContext _context) : IRequestHandler<GetUserJoinRequestsQuery, List<UserJoinRequestDTO>>
{








    public async Task<List<UserJoinRequestDTO>> Handle(GetUserJoinRequestsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.UserJoinRequests.AsNoTracking().Include(x => x.Firm).AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Status))
            query = query.Where(x => x.Status == request.Status);

        var requests = await query.OrderByDescending(x => x.RequestedAt).Select(x => new UserJoinRequestDTO
            {
                RequestID = x.RequestID,
                FirmID = x.FirmID,
                FirmName = x.Firm != null ? x.Firm.FirmName : null,
                UserID = x.UserID,
                FullName = x.FullName,
                Email = x.Email,
                Phone = x.Phone,
                Department = x.Department,
                RequestedRoleID = x.RequestedRoleID,
                Status = x.Status,
                RequestedAt = x.RequestedAt,
                ReviewedBy = x.ReviewedBy,
                ReviewedAt = x.ReviewedAt,
                RejectionReason = x.RejectionReason,
                CreatedUserID = x.CreatedUserID
            })
            .ToListAsync(cancellationToken);

        foreach (var dto in requests)
        {
            if (System.Enum.IsDefined(typeof(UserRole), dto.RequestedRoleID))
                dto.RequestedRoleName = ((UserRole)dto.RequestedRoleID).ToString();
        }




        var reviewerIds = requests.Where(x => x.ReviewedBy.HasValue).Select(x => x.ReviewedBy!.Value).Distinct().ToList();
        if (reviewerIds.Count > 0)
        {
            var reviewerNames = await _context.Users.AsNoTracking().IgnoreQueryFilters().Where(x => reviewerIds.Contains(x.UserID)).ToDictionaryAsync(x => x.UserID, x => x.FullName, cancellationToken);

            foreach (var dto in requests)
            {
                if (dto.ReviewedBy.HasValue && reviewerNames.TryGetValue(dto.ReviewedBy.Value, out var name))
                    dto.ReviewedByName = name;
            }
        }

        return requests;
    }
}
