using LTSBackend.Data;
using LTSBackend.Features.CaseCategories.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.CaseCategories.Queries.GetAllCaseCategories;

public sealed class GetAllCaseCategoriesHandler(AppDbContext _context, ILogger<GetAllCaseCategoriesHandler> _logger) : IRequestHandler<GetAllCaseCategoriesQuery, List<CaseCategoryDTO>>
{
    // =====================================================
    // HANDLE — lists categories for dropdowns / admin screens
    // Optionally filtered by search text and by active-only. Visibility
    // (global + own firm) is enforced by the entity's EF Core
    // HasQueryFilter, not by this handler.
    // =====================================================
    public async Task<List<CaseCategoryDTO>> Handle(GetAllCaseCategoriesQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching all case categories (SearchText={SearchText}, ActiveOnly={ActiveOnly})", request.SearchText, request.ActiveOnly);

        var query = _context.CaseCategories.AsNoTracking().AsQueryable();

        if (request.ActiveOnly)
        {
            query = query.Where(x => x.IsActive);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchText))
        {
            var search = request.SearchText.Trim().ToLower();
            query = query.Where(x => x.CategoryName.ToLower().Contains(search) || (x.Description != null && x.Description.ToLower().Contains(search)));
        }

        var categories = await query.OrderBy(x => x.CategoryName)
            .Select(x => new CaseCategoryDTO
            {
                CategoryID = x.CategoryID,
                CategoryName = x.CategoryName,
                Description = x.Description,
                IsActive = x.IsActive,
                IsGlobal = x.FirmID == null
            })
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Retrieved {Count} case categories", categories.Count);

        return categories;
    }
}
