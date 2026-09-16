using MediatR;

namespace LTSBackend.Features.Hearings.Commands.DeleteAttendance
{
    public class DeleteAttendanceCommand : IRequest<bool>
    {
        public long AttendanceId { get; set; }
    }
}
