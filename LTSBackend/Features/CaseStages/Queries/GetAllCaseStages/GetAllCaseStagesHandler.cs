using LTSBackend.Data;
using LTSBackend.Features.CaseStages.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.CaseStages.Queries.GetAllCaseStages;

public sealed class GetAllCaseStagesHandler(AppDbContext _context, ILogger<GetAllCaseStagesHandler> _logger) : IRequestHandler<GetAllCaseStagesQuery, List<CaseStageDTO>>
{
    // =====================================================
    // HANDLE — lists stages for dropdowns / admin screens
    // Optionally filtered by search text and by active-only. Visibility
    // (global + own firm) is enforced by the entity's EF Core
    // HasQueryFilter, not by this handler.
    // =====================================================
    public async Task<List<CaseStageDTO>> Handle(GetAllCaseStagesQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching all case stages (SearchText={SearchText}, ActiveOnly={ActiveOnly})", request.SearchText, request.ActiveOnly);

        var query = _context.CaseStages.AsNoTracking().AsQueryable();

        if (request.ActiveOnly)
            query = query.Where(x => x.IsActive);

        if (!string.IsNullOrWhiteSpace(request.SearchText))
        {
            var search = request.SearchText.Trim().ToLower();
            query = query.Where(x =>x.StageName.ToLower().Contains(search) || (x.Description != null && x.Description.ToLower().Contains(search)));
        }

        var stages = await query.OrderBy(x => x.StageName)
            .Select(x => new CaseStageDTO
            {
                StageID = x.StageID,
                StageName = x.StageName,
                Description = x.Description,
                IsActive = x.IsActive,
                IsGlobal = x.FirmID == null
            })
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Retrieved {Count} case stages", stages.Count);

        return stages;
    }
}
