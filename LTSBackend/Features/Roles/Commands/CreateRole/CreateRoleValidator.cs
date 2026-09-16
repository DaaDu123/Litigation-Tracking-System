using FluentValidation;
namespace LTSBackend.Features.Roles.Commands.CreateRole;

public class CreateRoleValidator : AbstractValidator<CreateRoleCommand>
{
    // Requires a role name (letters/numbers/spaces/hyphens/underscores
    // only) and at least one PermissionID.
    public CreateRoleValidator()
    {
        RuleFor(x => x.RoleName)
            .NotEmpty()
            .WithMessage("Role name is required.")
            .MaximumLength(50)
            .WithMessage("Role name cannot exceed 50 characters.")
            .Matches(@"^[a-zA-Z0-9\s_-]+$")
            .WithMessage("Role name can only contain letters, numbers, spaces, hyphens, and underscores.");

        RuleFor(x => x.Description)
            .MaximumLength(255)
            .WithMessage("Description cannot exceed 255 characters.");

        RuleFor(x => x.PermissionIds)
            .NotEmpty()
            .WithMessage("At least one permission is required.");
    }
}
