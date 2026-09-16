using MediatR;
using LTSBackend.Features.Hearings.DTOs;

namespace LTSBackend.Features.Hearings.Commands.UpdateHearing
{
    public class UpdateHearingCommand : IRequest<bool>
    {
        public UpdateHearingDTO Hearing { get; set; } = null!;
    }
}