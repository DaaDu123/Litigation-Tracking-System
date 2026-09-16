using FluentValidation;

namespace LTSBackend.Features.Hearings.Commands.UpdateHearing
{
    public class UpdateHearingValidator : AbstractValidator<UpdateHearingCommand>
    {
        // Requires a valid HearingID, a hearing date, a judge name, and a
        // hearing purpose. Unlike CreateHearingValidator, an already-scheduled
        // hearing's date is not re-checked against "today" here, since an
        // update may legitimately be recording a completed hearing's outcome.
        public UpdateHearingValidator()
        {
            RuleFor(x => x.Hearing).NotNull();

            RuleFor(x => x.Hearing.HearingId)
                .GreaterThan(0).WithMessage("Hearing ID must be greater than 0");

            RuleFor(x => x.Hearing.HearingDate)
                .NotEmpty().WithMessage("Hearing date is required");

            RuleFor(x => x.Hearing.JudgeName)
                .NotEmpty().WithMessage("Judge name is required")
                .MaximumLength(250).WithMessage("Judge name cannot exceed 250 characters");

            RuleFor(x => x.Hearing.HearingPurpose)
                .NotEmpty().WithMessage("Hearing purpose is required")
                .MaximumLength(500).WithMessage("Hearing purpose cannot exceed 500 characters");
        }
    }
}