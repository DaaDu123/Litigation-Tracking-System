using FluentValidation;

namespace LTSBackend.Features.Cases.Commands.GenerateCaseWorkflow;

public class GenerateCaseWorkflowValidator : AbstractValidator<GenerateCaseWorkflowCommand>
{
    public GenerateCaseWorkflowValidator()
    {
        RuleFor(x => x.CaseID).GreaterThan(0).WithMessage("Valid Case ID is required");
    }
}
