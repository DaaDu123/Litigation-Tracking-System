using FluentValidation;

namespace LTSBackend.Features.Users.Commands.ChangeUserRole;

public class ChangeUserRoleValidator : AbstractValidator<ChangeUserRoleCommand>
{
    public ChangeUserRoleValidator()
    {
        RuleFor(x => x.UserID).GreaterThan(0);
        RuleFor(x => x.NewRoleID).GreaterThan(0);
    }
}
