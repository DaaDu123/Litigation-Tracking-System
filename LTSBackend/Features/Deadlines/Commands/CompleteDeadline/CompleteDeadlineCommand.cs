using MediatR;

namespace LTSBackend.Features.Deadlines.Commands.CompleteDeadline
{
    public class CompleteDeadlineCommand : IRequest<bool>
    {
        public long DeadlineID { get; set; }
    }
}
