using FluentValidation;

namespace LTSBackend.Features.CaseStages.Commands.UpdateCaseStage;

public class UpdateCaseStageValidator : AbstractValidator<UpdateCaseStageCommand>
{
    // Requires a valid StageID plus the same name/description rules as
    // CreateCaseStageValidator.
    public UpdateCaseStageValidator()
    {
        RuleFor(x => x.StageID).GreaterThan(0).WithMessage("Valid stage ID is required.");

        RuleFor(x => x.StageName)
            .NotEmpty().WithMessage("Stage name is required.")
            .MaximumLength(150).WithMessage("Stage name cannot exceed 150 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.")
            .When(x => !string.IsNullOrEmpty(x.Description));
    }
}
