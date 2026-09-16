using MediatR;

namespace LTSBackend.Features.Hearings.Commands.DeleteHearing
{
    public class DeleteHearingCommand : IRequest<bool>
    {
        public long HearingId { get; set; }
    }
}