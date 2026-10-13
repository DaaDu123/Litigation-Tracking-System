using MediatR;

namespace LTSBackend.Features.Documents.Commands.ApproveDocument
{

    public record ApproveDocumentCommand(long DocumentID) : IRequest<bool>
    {
        public int UserID { get; init; }
    }
}
