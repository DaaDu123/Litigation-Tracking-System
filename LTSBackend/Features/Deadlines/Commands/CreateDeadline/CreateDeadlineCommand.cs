using MediatR;
using LTSBackend.Features.Deadlines.DTOs;

namespace LTSBackend.Features.Deadlines.Commands.CreateDeadline
{
    public class CreateDeadlineCommand : IRequest<long>
    {
        public CreateDeadlineDTO Deadline { get; set; } = null!;
    }
}
