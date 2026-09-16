using LTSBackend.Data;
using LTSBackend.Features.DocumentTypes.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.DocumentTypes.Queries.GetDocumentTypeOptions;

public sealed class GetDocumentTypeOptionsHandler(AppDbContext _context, ILogger<GetDocumentTypeOptionsHandler> _logger)
    : IRequestHandler<GetDocumentTypeOptionsQuery, List<DocumentTypeOptionDTO>>
{
    // =====================================================
    // HANDLE — lists active document types as {ID, Name} pairs only
    // Same tenant visibility (global + caller's own firm) as the full
    // GetAllDocumentTypesHandler, enforced by the EF Core HasQueryFilter
    // on DocumentType, not by this handler. The difference from
    // GetAllDocumentTypesHandler is purely the response shape (no
    // Description/IsActive/IsGlobal) and the controller-level role gate
    // that lets AssociateLawyer/Moharrir/InternParalegal call this one but
    // not the full master-data endpoint.
    // =====================================================
    public async Task<List<DocumentTypeOptionDTO>> Handle(GetDocumentTypeOptionsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching document type dropdown options");

        var options = await _context.DocumentTypes.AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.TypeName)
            .Select(x => new DocumentTypeOptionDTO
            {
                DocumentTypeID = x.DocumentTypeID,
                TypeName = x.TypeName
            })
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Retrieved {Count} document type options", options.Count);

        return options;
    }
}
