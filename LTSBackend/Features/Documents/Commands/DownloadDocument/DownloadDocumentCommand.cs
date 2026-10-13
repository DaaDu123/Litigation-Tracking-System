using MediatR;

namespace LTSBackend.Features.Documents.Commands.DownloadDocument
{

    public record DownloadDocumentCommand(long DocumentID) : IRequest<DocumentDownloadDTO>
    {
        public int UserID { get; init; }
    }
}
