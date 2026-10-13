using MediatR;
namespace LTSBackend.Features.Documents.Commands.UploadDocument
{

    public record UploadDocumentCommand(long CaseID, int DocumentTypeID, string DocumentName, IFormFile File, string? Remarks) : IRequest<UploadDocumentResult>
    {
        public int UserID { get; init; }
    }
}
