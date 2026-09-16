using LTSBackend.Features.Documents.DTOs;
using MediatR;

namespace LTSBackend.Features.Documents.Queries.GetCaseDocuments
{
    // Requests the list of documents attached to a case.
    public record GetCaseDocumentsQuery(long CaseID) : IRequest<List<DocumentDetailDTO>>
    {
        public int UserID { get; init; }
    }
}
