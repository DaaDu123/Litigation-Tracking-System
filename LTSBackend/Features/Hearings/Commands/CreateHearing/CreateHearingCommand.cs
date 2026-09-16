using MediatR;
using LTSBackend.Features.Hearings.DTOs;

namespace LTSBackend.Features.Hearings.Commands.CreateHearing
{
    public class CreateHearingCommand : IRequest<long>
    {
        public CreateHearingDTO Hearing { get; set; } = null!;
    }
}