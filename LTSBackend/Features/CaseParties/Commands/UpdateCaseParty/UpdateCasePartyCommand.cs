using MediatR;
using LTSBackend.Features.CaseParties.DTOs;

namespace LTSBackend.Features.CaseParties.Commands.UpdateCaseParty
{
    public class UpdateCasePartyCommand : IRequest<bool>
    {
        public UpdateCasePartyDTO Party { get; set; } = null!;
    }
}
