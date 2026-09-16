using MediatR;
using LTSBackend.Features.CaseParties.DTOs;

namespace LTSBackend.Features.CaseParties.Commands.CreateCaseParty
{
    public class CreateCasePartyCommand : IRequest<long>
    {
        public CreateCasePartyDTO Party { get; set; } = null!;
    }
}
