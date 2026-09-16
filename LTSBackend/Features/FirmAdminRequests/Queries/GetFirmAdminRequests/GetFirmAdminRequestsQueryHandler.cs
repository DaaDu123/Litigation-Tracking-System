using LTSBackend.Data;
using LTSBackend.Features.FirmAdminRequests.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.FirmAdminRequests.Queries.GetFirmAdminRequests;

public class GetFirmAdminRequestsQueryHandler(AppDbContext _context) : IRequestHandler<GetFirmAdminRequestsQuery, List<FirmAdminRequestDTO>>
{
    // =====================================================
    // HANDLE — lists Firm Admin requests, optionally filtered by status
    // Newest-first; resolves each reviewed request's reviewer name via a
    // small follow-up query.
    // =====================================================
    public async Task<List<FirmAdminRequestDTO>> Handle(GetFirmAdminRequestsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.FirmAdminRequests.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Status))
            query = query.Where(x => x.Status == request.Status);

        var requests = await query.OrderByDescending(x => x.RequestedAt).Select(x => new FirmAdminRequestDTO
            {
                RequestID = x.RequestID,
                FirmName = x.FirmName,
                FirmCode = x.FirmCode,
                Address = x.Address,
                ContactEmail = x.ContactEmail,
                ContactPhone = x.ContactPhone,
                AdminFullName = x.AdminFullName,
                AdminEmail = x.AdminEmail,
                AdminPhone = x.AdminPhone,
                Status = x.Status,
                RequestedAt = x.RequestedAt,
                ReviewedBy = x.ReviewedBy,
                ReviewedAt = x.ReviewedAt,
                RejectionReason = x.RejectionReason,
                CreatedFirmID = x.CreatedFirmID
            })
            .ToListAsync(cancellationToken);

        // Fill in the reviewer's name with a second, tiny query rather than
        // a join above - keeps the projection simple and this list is
        // small (Pending review queues don't grow large).
        var reviewerIds = requests.Where(x => x.ReviewedBy.HasValue).Select(x => x.ReviewedBy!.Value).Distinct().ToList();
        if (reviewerIds.Count > 0)
        {
            var reviewerNames = await _context.Users.AsNoTracking().Where(x => reviewerIds.Contains(x.UserID)).ToDictionaryAsync(x => x.UserID, x => x.FullName, cancellationToken);

            foreach (var dto in requests)
            {
                if (dto.ReviewedBy.HasValue && reviewerNames.TryGetValue(dto.ReviewedBy.Value, out var name))
                    dto.ReviewedByName = name;
            }
        }

        return requests;
    }
}
