using LTSBackend.Data;
using LTSBackend.Features.Courts.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.Courts.Queries.GetAllCourts;

public sealed class GetAllCourtsHandler(AppDbContext _context, ILogger<GetAllCourtsHandler> _logger) : IRequestHandler<GetAllCourtsQuery, List<CourtDTO>>
{
    // =====================================================
    // HANDLE — lists courts for dropdowns / admin screens
    // Optionally filtered by search text and by active-only. Visibility
    // (global + own firm) is enforced by the entity's EF Core
    // HasQueryFilter, not by this handler.
    // =====================================================
    public async Task<List<CourtDTO>> Handle(GetAllCourtsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching all courts (SearchText={SearchText}, ActiveOnly={ActiveOnly})", request.SearchText, request.ActiveOnly);

        var query = _context.Courts.AsNoTracking().AsQueryable();

        if (request.ActiveOnly)
        {
            query = query.Where(x => x.IsActive);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchText))
        {
            var search = request.SearchText.Trim().ToLower();
            query = query.Where(x => x.CourtName.ToLower().Contains(search) || (x.CourtType != null && x.CourtType.ToLower().Contains(search)) || (x.Jurisdiction != null && x.Jurisdiction.ToLower().Contains(search)));
        }

        var courts = await query.OrderBy(x => x.CourtName)
            .Select(x => new CourtDTO
            {
                CourtID = x.CourtID,
                CourtName = x.CourtName,
                CourtType = x.CourtType,
                Jurisdiction = x.Jurisdiction,
                Address = x.Address,
                IsActive = x.IsActive,
                CreatedDate = x.CreatedDate,
                IsGlobal = x.FirmID == null
            })
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Retrieved {Count} courts", courts.Count);

        return courts;
    }
}
