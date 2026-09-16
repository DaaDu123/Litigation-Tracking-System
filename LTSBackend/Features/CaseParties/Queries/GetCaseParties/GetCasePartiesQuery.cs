using MediatR;
using LTSBackend.Features.CaseParties.DTOs;

namespace LTSBackend.Features.CaseParties.Queries.GetCaseParties
{
    public class GetCasePartiesQuery : IRequest<List<CasePartyDetailDTO>>
    {
        public long CaseID { get; set; }
    }
}
