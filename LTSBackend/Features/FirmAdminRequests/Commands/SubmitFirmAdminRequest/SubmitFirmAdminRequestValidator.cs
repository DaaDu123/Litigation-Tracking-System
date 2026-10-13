using FluentValidation;

namespace LTSBackend.Features.FirmAdminRequests.Commands.SubmitFirmAdminRequest;

public class SubmitFirmAdminRequestValidator : AbstractValidator<SubmitFirmAdminRequestCommand>
{



    public SubmitFirmAdminRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(150);

        RuleFor(x => x.Password)
            .NotEmpty().MinimumLength(8)
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).+$")
            .WithMessage("Password must include uppercase, lowercase, a digit, and a symbol.");
    }
}
