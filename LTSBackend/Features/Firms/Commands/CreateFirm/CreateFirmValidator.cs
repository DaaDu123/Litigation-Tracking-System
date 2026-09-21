using FluentValidation;

namespace LTSBackend.Features.Firms.Commands.CreateFirm;

public class CreateFirmValidator : AbstractValidator<CreateFirmCommand>
{
    // Requires the firm's name and its first FirmAdmin's name/email/
    // password (standard complexity rules). FirmCode is generated
    // internally by the handler, never entered by the SuperAdmin.
    public CreateFirmValidator()
    {
        RuleFor(x => x.FirmName).NotEmpty().MaximumLength(150);

        RuleFor(x => x.ContactEmail).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.ContactEmail));

        RuleFor(x => x.AdminFullName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.AdminEmail).NotEmpty().EmailAddress();
        RuleFor(x => x.AdminPassword)
            .NotEmpty().MinimumLength(8)
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).+$")
            .WithMessage("Password must include uppercase, lowercase, a digit, and a symbol.");
    }
}
