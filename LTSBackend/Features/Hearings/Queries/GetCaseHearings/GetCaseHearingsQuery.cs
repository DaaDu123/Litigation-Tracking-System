using MediatR;
using LTSBackend.Features.Hearings.DTOs;

namespace LTSBackend.Features.Hearings.Queries.GetCaseHearings
{
    public class GetCaseHearingsQuery : IRequest<PagedHearingResult<HearingDetailDTO>>
    {
        public long CaseId { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}