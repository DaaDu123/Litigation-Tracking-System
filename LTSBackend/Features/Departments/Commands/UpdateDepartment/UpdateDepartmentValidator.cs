using FluentValidation;

namespace LTSBackend.Features.Departments.Commands.UpdateDepartment;

public class UpdateDepartmentValidator : AbstractValidator<UpdateDepartmentCommand>
{
    // Requires a valid DepartmentID plus the same name/code/description
    // rules as CreateDepartmentValidator.
    public UpdateDepartmentValidator()
    {
        RuleFor(x => x.DepartmentID)
            .GreaterThan(0)
            .WithMessage("Valid department ID is required.");

        RuleFor(x => x.DepartmentName)
            .NotEmpty()
            .WithMessage("Department name is required.")
            .MaximumLength(100)
            .WithMessage("Department name cannot exceed 100 characters.");

        RuleFor(x => x.DepartmentCode)
            .MaximumLength(20)
            .WithMessage("Department code cannot exceed 20 characters.")
            .Matches(@"^[a-zA-Z0-9_-]+$")
            .WithMessage("Department code can only contain letters, numbers, hyphens, and underscores.")
            .When(x => !string.IsNullOrWhiteSpace(x.DepartmentCode));

        RuleFor(x => x.Description)
            .MaximumLength(255)
            .WithMessage("Description cannot exceed 255 characters.");
    }
}
