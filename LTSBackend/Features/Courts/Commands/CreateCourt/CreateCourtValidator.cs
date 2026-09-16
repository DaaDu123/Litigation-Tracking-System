using FluentValidation;

namespace LTSBackend.Features.Courts.Commands.CreateCourt;

public class CreateCourtValidator : AbstractValidator<CreateCourtCommand>
{
    // Requires a court name; type/jurisdiction/address are optional but
    // length-capped if supplied.
    public CreateCourtValidator()
    {
        RuleFor(x => x.CourtName)
            .NotEmpty()
            .WithMessage("Court name is required.")
            .MaximumLength(150)
            .WithMessage("Court name cannot exceed 150 characters.");

        RuleFor(x => x.CourtType)
            .MaximumLength(100)
            .WithMessage("Court type cannot exceed 100 characters.")
            .When(x => !string.IsNullOrEmpty(x.CourtType));

        RuleFor(x => x.Jurisdiction)
            .MaximumLength(200)
            .WithMessage("Jurisdiction cannot exceed 200 characters.")
            .When(x => !string.IsNullOrEmpty(x.Jurisdiction));

        RuleFor(x => x.Address)
            .MaximumLength(500)
            .WithMessage("Address cannot exceed 500 characters.")
            .When(x => !string.IsNullOrEmpty(x.Address));
    }
}
