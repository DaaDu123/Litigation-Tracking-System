using FluentValidation;

namespace LTSBackend.Features.Hearings.Commands.CreateHearing
{
    public class CreateHearingValidator : AbstractValidator<CreateHearingCommand>
    {
        // Requires a valid Case/Court ID, a Hearing Date that is not in the
        // past, a judge name, and a hearing purpose.
        public CreateHearingValidator()
        {
            RuleFor(x => x.Hearing).NotNull();

            RuleFor(x => x.Hearing.CaseId)
                .GreaterThan(0).WithMessage("Case ID must be greater than 0");

            RuleFor(x => x.Hearing.CourtId)
                .GreaterThan(0).WithMessage("Court ID must be greater than 0");

            RuleFor(x => x.Hearing.HearingDate)
                .NotEmpty().WithMessage("Hearing date is required")
                .GreaterThanOrEqualTo(System.DateTime.UtcNow).WithMessage("Hearing date must be in the future");

            RuleFor(x => x.Hearing.JudgeName)
                .NotEmpty().WithMessage("Judge name is required")
                .MaximumLength(250).WithMessage("Judge name cannot exceed 250 characters");

            RuleFor(x => x.Hearing.HearingPurpose)
                .NotEmpty().WithMessage("Hearing purpose is required")
                .MaximumLength(500).WithMessage("Hearing purpose cannot exceed 500 characters");
        }
    }
}