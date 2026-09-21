using FluentValidation;

namespace LTSBackend.Features.Users.Commands.RemoveFirmUser;

public class RemoveFirmUserValidator : AbstractValidator<RemoveFirmUserCommand>
{
    public RemoveFirmUserValidator()
    {
        RuleFor(x => x.UserID).GreaterThan(0);
        RuleFor(x => x.Reason).NotEmpty().WithMessage("A reason is required.").MaximumLength(500);
    }
}
