using FluentValidation;

namespace LTSBackend.Features.CaseWorkflowTemplates.Commands.SaveCaseWorkflowTemplate;

public class SaveCaseWorkflowTemplateValidator : AbstractValidator<SaveCaseWorkflowTemplateCommand>
{
    // Structural rules only (existence/ownership/active checks need the DB
    // and live in the handler): category + initial status are required,
    // at least one stage, no duplicate stage or document type, and a
    // document type can't be both required and optional.
    public SaveCaseWorkflowTemplateValidator()
    {
        RuleFor(x => x.CategoryID).GreaterThan(0).WithMessage("Case Category is required");

        RuleFor(x => x.InitialStatusID).GreaterThan(0).WithMessage("Initial Status is required");

        RuleFor(x => x.DefaultDepartmentID)
            .GreaterThan(0).WithMessage("A valid Department is required - not 0 or a negative ID")
            .When(x => x.DefaultDepartmentID.HasValue);

        RuleFor(x => x.StageIDs)
            .NotNull().WithMessage("Stages are required")
            .Must(l => l != null && l.Count > 0).WithMessage("Add at least one workflow stage")
            .Must(l => l == null || l.Distinct().Count() == l.Count).WithMessage("A stage can only appear once in a workflow (stage order must be unique)")
            .Must(l => l == null || l.All(i => i > 0)).WithMessage("Every stage must be a valid Stage");

        RuleFor(x => x.RequiredDocumentTypeIDs)
            .NotNull()
            .Must(l => l == null || l.Distinct().Count() == l.Count).WithMessage("A required Document Type can only be listed once")
            .Must(l => l == null || l.All(i => i > 0)).WithMessage("Every required Document Type must be valid");

        RuleFor(x => x.OptionalDocumentTypeIDs)
            .NotNull()
            .Must(l => l == null || l.Distinct().Count() == l.Count).WithMessage("An optional Document Type can only be listed once")
            .Must(l => l == null || l.All(i => i > 0)).WithMessage("Every optional Document Type must be valid");

        RuleFor(x => x)
            .Must(x => x.RequiredDocumentTypeIDs == null || x.OptionalDocumentTypeIDs == null
                       || !x.RequiredDocumentTypeIDs.Intersect(x.OptionalDocumentTypeIDs).Any())
            .WithMessage("A Document Type cannot be both required and optional")
            .OverridePropertyName("OptionalDocumentTypeIDs");
    }
}
