using MediatR;
namespace LTSBackend.Features.Documents.Commands.UploadDocument
{
    // Carries a new document file plus its metadata for a case upload.
    public record UploadDocumentCommand(long CaseID, int DocumentTypeID, string DocumentName, IFormFile File, string? Remarks) : IRequest<UploadDocumentResult>
    {
        public int UserID { get; init; }
    }
}
