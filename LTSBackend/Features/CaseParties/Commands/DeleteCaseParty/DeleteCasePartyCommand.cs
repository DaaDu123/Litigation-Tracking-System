using MediatR;

namespace LTSBackend.Features.CaseParties.Commands.DeleteCaseParty
{
    public class DeleteCasePartyCommand : IRequest<bool>
    {
        public long PartyID { get; set; }
    }
}
