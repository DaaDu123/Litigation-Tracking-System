using MediatR;
using LTSBackend.Features.CaseParties.DTOs;

namespace LTSBackend.Features.CaseParties.Queries.GetCasePartyById
{
    public class GetCasePartyByIdQuery : IRequest<CasePartyDetailDTO>
    {
        public long PartyID { get; set; }
    }
}
