using FluentValidation;

namespace LTSBackend.Features.Users.Commands.BlockFirmUser;

public class BlockFirmUserValidator : AbstractValidator<BlockFirmUserCommand>
{
    public BlockFirmUserValidator()
    {
        RuleFor(x => x.UserID).GreaterThan(0);
        RuleFor(x => x.Reason).NotEmpty().WithMessage("A reason is required.").MaximumLength(500);
    }
}
