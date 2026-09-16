using MediatR;
using LTSBackend.Features.CaseNotes.DTOs;

namespace LTSBackend.Features.CaseNotes.Commands.CreateNote
{
    public class CreateCaseNoteCommand : IRequest<long>
    {
        public CreateCaseNoteDTO Note { get; set; } = null!;
    }
}
