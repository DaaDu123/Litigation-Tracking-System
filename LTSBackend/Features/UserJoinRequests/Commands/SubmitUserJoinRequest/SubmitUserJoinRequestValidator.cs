using FluentValidation;

namespace LTSBackend.Features.UserJoinRequests.Commands.SubmitUserJoinRequest;

public class SubmitUserJoinRequestValidator : AbstractValidator<SubmitUserJoinRequestCommand>
{
    public SubmitUserJoinRequestValidator()
    {
        RuleFor(x => x.FirmID).GreaterThan(0).WithMessage("A firm must be selected.");
    }
}
