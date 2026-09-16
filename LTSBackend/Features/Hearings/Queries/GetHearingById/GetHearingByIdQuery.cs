using MediatR;
using LTSBackend.Features.Hearings.DTOs;

namespace LTSBackend.Features.Hearings.Queries.GetHearingById
{
    public class GetHearingByIdQuery : IRequest<HearingDetailDTO>
    {
        public long HearingId { get; set; }
    }
}