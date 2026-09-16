using FluentValidation;

namespace LTSBackend.Features.Departments.Commands.CreateDepartment;

public class CreateDepartmentValidator : AbstractValidator<CreateDepartmentCommand>
{
    // Requires a department name; DepartmentCode, if supplied, must be
    // alphanumeric (plus hyphen/underscore) and length-capped.
    public CreateDepartmentValidator()
    {
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
