using FluentValidation;

namespace LTSBackend.Features.FirmAdminRequests.Commands.SubmitFirmAdminRequest;

public class SubmitFirmAdminRequestValidator : AbstractValidator<SubmitFirmAdminRequestCommand>
{
    // Requires the firm's name/code (letters, numbers, hyphens only) and
    // the requesting admin's name/email/password (standard complexity
    // rules) — the same shape as CreateFirmValidator, since this is the
    // self-service version of that same operation.
    public SubmitFirmAdminRequestValidator()
    {
        RuleFor(x => x.FirmName).NotEmpty().MaximumLength(150);

        RuleFor(x => x.FirmCode)
            .NotEmpty().MaximumLength(30)
            .Matches("^[A-Za-z0-9-]+$").WithMessage("Firm code can only contain letters, numbers, and hyphens.");

        RuleFor(x => x.ContactEmail).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.ContactEmail));

        RuleFor(x => x.AdminFullName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.AdminEmail).NotEmpty().EmailAddress();
        RuleFor(x => x.AdminPassword)
            .NotEmpty().MinimumLength(8)
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).+$")
            .WithMessage("Password must include uppercase, lowercase, a digit, and a symbol.");
    }
}
