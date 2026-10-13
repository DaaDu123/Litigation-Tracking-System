using LTSBackend.Features.Documents.DTOs;
using MediatR;

namespace LTSBackend.Features.Documents.Queries.GetDocument
{

    public record GetDocumentQuery(long DocumentID) : IRequest<DocumentDetailDTO?>
    {
        public int UserID { get; init; }
    }
}
