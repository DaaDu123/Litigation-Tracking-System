using MediatR;
using LTSBackend.Features.Hearings.DTOs;

namespace LTSBackend.Features.Hearings.Queries.GetHearingAttendance
{
    public class GetHearingAttendanceQuery : IRequest<List<HearingAttendanceDTO>>
    {
        public long HearingId { get; set; }
    }
}
