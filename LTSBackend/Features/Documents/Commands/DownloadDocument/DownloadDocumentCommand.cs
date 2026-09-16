using MediatR;

namespace LTSBackend.Features.Documents.Commands.DownloadDocument
{
    // Requests the raw bytes of a document for download.
    public record DownloadDocumentCommand(long DocumentID) : IRequest<DocumentDownloadDTO>
    {
        public int UserID { get; init; }
    }
}
