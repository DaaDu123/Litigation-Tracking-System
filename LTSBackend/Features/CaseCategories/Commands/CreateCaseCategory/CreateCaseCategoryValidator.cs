using FluentValidation;

namespace LTSBackend.Features.CaseCategories.Commands.CreateCaseCategory;

public class CreateCaseCategoryValidator : AbstractValidator<CreateCaseCategoryCommand>
{
    // Requires a category name (max 150 chars) and an optional description
    // (max 255 chars).
    public CreateCaseCategoryValidator()
    {
        RuleFor(x => x.CategoryName)
            .NotEmpty()
            .WithMessage("Category name is required.")
            .MaximumLength(150)
            .WithMessage("Category name cannot exceed 150 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(255)
            .WithMessage("Description cannot exceed 255 characters.")
            .When(x => !string.IsNullOrEmpty(x.Description));
    }
}
