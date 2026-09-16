using FluentValidation;

namespace LTSBackend.Features.CaseCategories.Commands.UpdateCaseCategory;

public class UpdateCaseCategoryValidator : AbstractValidator<UpdateCaseCategoryCommand>
{
    // Requires a valid CategoryID plus the same name/description rules as
    // CreateCaseCategoryValidator.
    public UpdateCaseCategoryValidator()
    {
        RuleFor(x => x.CategoryID).GreaterThan(0).WithMessage("Valid category ID is required.");

        RuleFor(x => x.CategoryName)
            .NotEmpty().WithMessage("Category name is required.")
            .MaximumLength(150).WithMessage("Category name cannot exceed 150 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(255).WithMessage("Description cannot exceed 255 characters.")
            .When(x => !string.IsNullOrEmpty(x.Description));
    }
}
