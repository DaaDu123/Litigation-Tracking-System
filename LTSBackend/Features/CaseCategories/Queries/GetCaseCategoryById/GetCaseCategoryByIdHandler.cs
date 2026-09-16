using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using LTSBackend.Features.CaseCategories.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.CaseCategories.Queries.GetCaseCategoryById;

public sealed class GetCaseCategoryByIdHandler(AppDbContext _context, ILogger<GetCaseCategoryByIdHandler> _logger) : IRequestHandler<GetCaseCategoryByIdQuery, CaseCategoryDTO>
{
    // =====================================================
    // HANDLE — fetches a single case category by ID
    // =====================================================
    public async Task<CaseCategoryDTO> Handle(GetCaseCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        var category = await _context.CaseCategories.AsNoTracking().FirstOrDefaultAsync(x => x.CategoryID == request.CategoryID, cancellationToken);

        if (category == null)
        {
            _logger.LogWarning("Case category not found: {CategoryID}", request.CategoryID);
            throw new NotFoundException("Case category not found.");
        }

        return new CaseCategoryDTO
        {
            CategoryID = category.CategoryID,
            CategoryName = category.CategoryName,
            Description = category.Description,
            IsActive = category.IsActive,
            IsGlobal = category.FirmID == null
        };
    }
}
