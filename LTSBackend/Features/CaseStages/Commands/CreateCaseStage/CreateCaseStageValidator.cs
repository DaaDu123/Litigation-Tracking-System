using FluentValidation;

namespace LTSBackend.Features.CaseStages.Commands.CreateCaseStage;

public class CreateCaseStageValidator : AbstractValidator<CreateCaseStageCommand>
{
    // Requires a stage name (max 150 chars) and an optional description
    // (max 500 chars).
    public CreateCaseStageValidator()
    {
        RuleFor(x => x.StageName)
            .NotEmpty().WithMessage("Stage name is required.")
            .MaximumLength(150).WithMessage("Stage name cannot exceed 150 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.")
            .When(x => !string.IsNullOrEmpty(x.Description));
    }
}
