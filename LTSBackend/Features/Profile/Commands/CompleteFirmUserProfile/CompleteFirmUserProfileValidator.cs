using FluentValidation;
using LTSBackend.Comman.Validation;

namespace LTSBackend.Features.Profile.Commands.CompleteFirmUserProfile;

public class CompleteFirmUserProfileValidator : AbstractValidator<CompleteFirmUserProfileCommand>
{
    public CompleteFirmUserProfileValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(150);

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Contact number is required.")
            .Must(PakistaniFormat.IsValidPhone)
            .WithMessage("Enter a valid Pakistani mobile number (e.g. 03001234567 or +923001234567).");

        RuleFor(x => x.CNIC)
            .NotEmpty().WithMessage("CNIC is required.")
            .Must(PakistaniFormat.IsValidCnic)
            .WithMessage("Enter a valid CNIC (format: 35202-1234567-1).");
    }
}
