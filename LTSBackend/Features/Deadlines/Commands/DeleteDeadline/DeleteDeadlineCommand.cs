using MediatR;

namespace LTSBackend.Features.Deadlines.Commands.DeleteDeadline
{
    public class DeleteDeadlineCommand : IRequest<bool>
    {
        public long DeadlineID { get; set; }
    }
}
