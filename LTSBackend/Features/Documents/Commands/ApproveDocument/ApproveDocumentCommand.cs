using MediatR;

namespace LTSBackend.Features.Documents.Commands.ApproveDocument
{
    // Marks a draft document (uploaded by Intern/Paralegal) as approved.
    public record ApproveDocumentCommand(long DocumentID) : IRequest<bool>
    {
        public int UserID { get; init; }
    }
}
