using MediatR;
using LTSBackend.Features.Deadlines.DTOs;

namespace LTSBackend.Features.Deadlines.Commands.UpdateDeadline
{
    public class UpdateDeadlineCommand : IRequest<bool>
    {
        public UpdateDeadlineDTO Deadline { get; set; } = null!;
    }
}
