using FluentValidation;

namespace LTSBackend.Features.Users.Commands.SetAvailability;

public class SetAvailabilityValidator : AbstractValidator<SetAvailabilityCommand>
{
    // Backend validation is mandatory (frontend validation is only a
    // convenience) - reason and a positive duration are both required
    // whenever the Firm Admin is going Inactive.
    public SetAvailabilityValidator()
    {
        When(x => !x.IsAvailable, () =>
        {
            RuleFor(x => x.Reason)
                .NotEmpty().WithMessage("Inactive reason is required.")
                .MaximumLength(500);

            RuleFor(x => x.Days).GreaterThanOrEqualTo(0).WithMessage("Duration cannot be negative.");
            RuleFor(x => x.Hours).InclusiveBetween(0, 23).WithMessage("Hours must be between 0 and 23.");
            RuleFor(x => x.Minutes).InclusiveBetween(0, 59).WithMessage("Minutes must be between 0 and 59.");

            RuleFor(x => x)
                .Must(x => x.Days > 0 || x.Hours > 0 || x.Minutes > 0)
                .WithMessage("Inactive duration is required and must be greater than zero.")
                .OverridePropertyName("Duration");
        });
    }
}
