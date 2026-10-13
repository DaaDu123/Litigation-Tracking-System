using MediatR;

namespace LTSBackend.Features.Documents.Commands.DeleteDocument
{

    public record DeleteDocumentCommand(long DocumentID) : IRequest<bool>
    {
        public int UserID { get; init; }
    }
}
