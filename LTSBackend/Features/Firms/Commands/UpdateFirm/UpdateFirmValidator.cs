using FluentValidation;

namespace LTSBackend.Features.Firms.Commands.UpdateFirm;

public class UpdateFirmValidator : AbstractValidator<UpdateFirmCommand>
{

    public UpdateFirmValidator()
    {
        RuleFor(x => x.FirmName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.ContactEmail).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.ContactEmail));
    }
}
