using FluentValidation;

namespace LTSBackend.Features.Hearings.Commands.UpdateAttendance
{
    public class UpdateAttendanceValidator : AbstractValidator<UpdateAttendanceCommand>
    {
        // Requires a valid AttendanceID, and — if both are supplied — a
        // DepartureTime that isn't earlier than the ArrivalTime.
        public UpdateAttendanceValidator()
        {
            RuleFor(x => x.AttendanceId).GreaterThan(0);
            RuleFor(x => x.AttendanceRole).MaximumLength(100);
            RuleFor(x => x.Remarks).MaximumLength(255);
            RuleFor(x => x.DepartureTime)
                .GreaterThanOrEqualTo(x => x.ArrivalTime)
                .When(x => x.ArrivalTime.HasValue && x.DepartureTime.HasValue)
                .WithMessage("Departure time cannot be before arrival time");
        }
    }
}
