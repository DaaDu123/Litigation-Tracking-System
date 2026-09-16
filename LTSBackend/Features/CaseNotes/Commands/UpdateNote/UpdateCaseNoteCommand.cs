using MediatR;
using LTSBackend.Features.CaseNotes.DTOs;

namespace LTSBackend.Features.CaseNotes.Commands.UpdateNote
{
    public class UpdateCaseNoteCommand : IRequest<bool>
    {
        public UpdateCaseNoteDTO Note { get; set; } = null!;
    }
}
