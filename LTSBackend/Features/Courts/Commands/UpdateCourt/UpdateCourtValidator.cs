using FluentValidation;

namespace LTSBackend.Features.Courts.Commands.UpdateCourt;

public class UpdateCourtValidator : AbstractValidator<UpdateCourtCommand>
{
    // Requires a valid CourtID plus the same name/type/jurisdiction/address
    // rules as CreateCourtValidator.
    public UpdateCourtValidator()
    {
        RuleFor(x => x.CourtID)
            .GreaterThan(0)
            .WithMessage("Valid court ID is required.");

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
