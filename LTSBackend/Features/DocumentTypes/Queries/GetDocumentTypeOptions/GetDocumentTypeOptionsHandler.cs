using LTSBackend.Data;
using LTSBackend.Features.DocumentTypes.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.DocumentTypes.Queries.GetDocumentTypeOptions;

public sealed class GetDocumentTypeOptionsHandler(AppDbContext _context, ILogger<GetDocumentTypeOptionsHandler> _logger)
    : IRequestHandler<GetDocumentTypeOptionsQuery, List<DocumentTypeOptionDTO>>
{










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
