using LTSBackend.Data;
using LTSBackend.Features.DocumentTypes.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.DocumentTypes.Queries.GetAllDocumentTypes;

public sealed class GetAllDocumentTypesHandler(AppDbContext _context, ILogger<GetAllDocumentTypesHandler> _logger) : IRequestHandler<GetAllDocumentTypesQuery, List<DocumentTypeDTO>>
{






    public async Task<List<DocumentTypeDTO>> Handle(GetAllDocumentTypesQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching all document types (SearchText={SearchText}, ActiveOnly={ActiveOnly})", request.SearchText, request.ActiveOnly);

        var query = _context.DocumentTypes.AsNoTracking().AsQueryable();

        if (request.ActiveOnly)
            query = query.Where(x => x.IsActive);

        if (!string.IsNullOrWhiteSpace(request.SearchText))
        {
            var search = request.SearchText.Trim().ToLower();
            query = query.Where(x =>x.TypeName.ToLower().Contains(search) || (x.Description != null && x.Description.ToLower().Contains(search)));
        }

        var types = await query.OrderBy(x => x.TypeName)
            .Select(x => new DocumentTypeDTO
            {
                DocumentTypeID = x.DocumentTypeID,
                TypeName = x.TypeName,
                Description = x.Description,
                IsActive = x.IsActive,
                IsGlobal = x.FirmID == null
            })
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Retrieved {Count} document types", types.Count);

        return types;
    }
}
