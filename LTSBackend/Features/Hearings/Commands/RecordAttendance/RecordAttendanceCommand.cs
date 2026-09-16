using MediatR;
using LTSBackend.Features.Hearings.DTOs;

namespace LTSBackend.Features.Hearings.Commands.RecordAttendance
{
    public class RecordAttendanceCommand : IRequest<long>
    {
        public RecordAttendanceDTO Attendance { get; set; } = null!;
    }
}
