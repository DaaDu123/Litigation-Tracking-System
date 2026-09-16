using LTSBackend.Features.Documents.DTOs;
using MediatR;

namespace LTSBackend.Features.Documents.Queries.GetDocument
{
    // Requests a single document's metadata by ID.
    public record GetDocumentQuery(long DocumentID) : IRequest<DocumentDetailDTO?>
    {
        public int UserID { get; init; }
    }
}
