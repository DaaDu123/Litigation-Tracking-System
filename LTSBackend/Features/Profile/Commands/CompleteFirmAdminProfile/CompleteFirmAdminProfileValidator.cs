using FluentValidation;
using LTSBackend.Comman.Validation;

namespace LTSBackend.Features.Profile.Commands.CompleteFirmAdminProfile;

public class CompleteFirmAdminProfileValidator : AbstractValidator<CompleteFirmAdminProfileCommand>
{
    public CompleteFirmAdminProfileValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.FirmName).NotEmpty().MaximumLength(150);

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Contact number is required.")
            .Must(PakistaniFormat.IsValidPhone)
            .WithMessage("Enter a valid Pakistani mobile number (e.g. 03001234567 or +923001234567).");

        RuleFor(x => x.CNIC)
            .NotEmpty().WithMessage("CNIC is required.")
            .Must(PakistaniFormat.IsValidCnic)
            .WithMessage("Enter a valid CNIC (format: 35202-1234567-1).");

        RuleFor(x => x.ContactEmail).EmailAddress().MaximumLength(150).When(x => !string.IsNullOrWhiteSpace(x.ContactEmail));
        RuleFor(x => x.ContactPhone).Must(PakistaniFormat.IsValidPhone).WithMessage("Enter a valid Pakistani mobile number.").When(x => !string.IsNullOrWhiteSpace(x.ContactPhone));
        RuleFor(x => x.Address).MaximumLength(255);
    }
}
