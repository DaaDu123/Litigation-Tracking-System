using FluentValidation;

namespace LTSBackend.Features.Hearings.Commands.RecordAttendance
{
    public class RecordAttendanceValidator : AbstractValidator<RecordAttendanceCommand>
    {
        // Requires a valid HearingID and UserID; role/remarks are optional but
        // length-capped.
        public RecordAttendanceValidator()
        {
            RuleFor(x => x.Attendance).NotNull();
            RuleFor(x => x.Attendance.HearingId).GreaterThan(0);
            RuleFor(x => x.Attendance.UserId).GreaterThan(0);
            RuleFor(x => x.Attendance.AttendanceRole).MaximumLength(100);
            RuleFor(x => x.Attendance.Remarks).MaximumLength(255);
        }
    }
}
