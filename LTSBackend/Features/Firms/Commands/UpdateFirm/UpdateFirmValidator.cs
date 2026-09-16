using FluentValidation;

namespace LTSBackend.Features.Firms.Commands.UpdateFirm;

public class UpdateFirmValidator : AbstractValidator<UpdateFirmCommand>
{
    // Requires a firm name and, if supplied, a well-formed contact email.
    public UpdateFirmValidator()
    {
        RuleFor(x => x.FirmName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.ContactEmail).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.ContactEmail));
    }
}
