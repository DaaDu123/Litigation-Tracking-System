using MediatR;

namespace LTSBackend.Features.Documents.Commands.DeleteDocument
{
    // Hard-deletes a document and its permissions/file.
    public record DeleteDocumentCommand(long DocumentID) : IRequest<bool>
    {
        public int UserID { get; init; }
    }
}
