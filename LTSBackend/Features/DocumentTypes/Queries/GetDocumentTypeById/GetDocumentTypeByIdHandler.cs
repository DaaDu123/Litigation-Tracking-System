using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using LTSBackend.Features.DocumentTypes.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.DocumentTypes.Queries.GetDocumentTypeById;

public sealed class GetDocumentTypeByIdHandler(AppDbContext _context, ILogger<GetDocumentTypeByIdHandler> _logger) : IRequestHandler<GetDocumentTypeByIdQuery, DocumentTypeDTO>
{
    // =====================================================
    // HANDLE — fetches a single document type by ID
    // =====================================================
    public async Task<DocumentTypeDTO> Handle(GetDocumentTypeByIdQuery request, CancellationToken cancellationToken)
    {
        var type = await _context.DocumentTypes.AsNoTracking().FirstOrDefaultAsync(x => x.DocumentTypeID == request.DocumentTypeID, cancellationToken);

        if (type == null)
        {
            _logger.LogWarning("Document type not found: {DocumentTypeID}", request.DocumentTypeID);
            throw new NotFoundException("Document type not found.");
        }

        return new DocumentTypeDTO
        {
            DocumentTypeID = type.DocumentTypeID,
            TypeName = type.TypeName,
            Description = type.Description,
            IsActive = type.IsActive,
            IsGlobal = type.FirmID == null
        };
    }
}
