using FluentValidation;

namespace LTSBackend.Features.Permissions.Commands.AssignPermissions;

public sealed class AssignPermissionsValidator
    : AbstractValidator<AssignPermissionsCommand>
{
    // Requires a valid RoleID and at least one PermissionID, with no
    // duplicates in the list.
    public AssignPermissionsValidator()
    {
        RuleFor(x => x.RoleID)
            .GreaterThan(0);

        RuleFor(x => x.PermissionIds)
            .NotNull()
            .NotEmpty()
            .Must(x => x.Distinct().Count() == x.Count)
            .WithMessage("Duplicate permissions are not allowed.");
    }
}