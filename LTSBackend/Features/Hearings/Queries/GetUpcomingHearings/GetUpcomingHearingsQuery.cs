using MediatR;
using LTSBackend.Features.Hearings.DTOs;

namespace LTSBackend.Features.Hearings.Queries.GetUpcomingHearings
{
    public class GetUpcomingHearingsQuery : IRequest<PagedHearingResult<HearingDetailDTO>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public long? CaseId { get; set; }
        public int? CourtId { get; set; }
    }
}