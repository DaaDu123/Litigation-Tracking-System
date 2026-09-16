using MediatR;

namespace LTSBackend.Features.Hearings.Commands.UpdateAttendance
{
    public class UpdateAttendanceCommand : IRequest<bool>
    {
        public long AttendanceId { get; set; }
        public bool IsPresent { get; set; }
        public string? AttendanceRole { get; set; }
        public DateTime? ArrivalTime { get; set; }
        public DateTime? DepartureTime { get; set; }
        public string? Remarks { get; set; }
    }
}
