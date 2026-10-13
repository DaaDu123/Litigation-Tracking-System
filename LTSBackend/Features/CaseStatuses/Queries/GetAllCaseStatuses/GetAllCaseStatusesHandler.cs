using LTSBackend.Data;
using LTSBackend.Features.CaseStatuses.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.CaseStatuses.Queries.GetAllCaseStatuses;

public sealed class GetAllCaseStatusesHandler(AppDbContext _context, ILogger<GetAllCaseStatusesHandler> _logger) : IRequestHandler<GetAllCaseStatusesQuery, List<CaseStatusDTO>>
{






    public async Task<List<CaseStatusDTO>> Handle(GetAllCaseStatusesQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching all case statuses (SearchText={SearchText}, ActiveOnly={ActiveOnly})", request.SearchText, request.ActiveOnly);

        var query = _context.CaseStatuses.AsNoTracking().AsQueryable();

        if (request.ActiveOnly)
            query = query.Where(x => x.IsActive);

        if (!string.IsNullOrWhiteSpace(request.SearchText))
        {
            var search = request.SearchText.Trim().ToLower();
            query = query.Where(x => x.StatusName.ToLower().Contains(search));
        }

        var statuses = await query.OrderBy(x => x.SequenceNo)
            .Select(x => new CaseStatusDTO
            {
                StatusID = x.StatusID,
                StatusName = x.StatusName,
                SequenceNo = x.SequenceNo,
                ColorCode = x.ColorCode,
                IsClosed = x.IsClosed,
                IsActive = x.IsActive,
                IsGlobal = x.FirmID == null
            })
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Retrieved {Count} case statuses", statuses.Count);

        return statuses;
    }
}
