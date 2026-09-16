using MediatR;

namespace LTSBackend.Features.CaseNotes.Commands.DeleteNote
{
    public class DeleteCaseNoteCommand : IRequest<bool>
    {
        public long NoteID { get; set; }
    }
}
