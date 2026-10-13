using FluentValidation;
using LTSBackend.Comman.Validation;

namespace LTSBackend.Features.Profile.Commands;

public class UpdateMyProfileValidator : AbstractValidator<UpdateMyProfileCommand>
{









    public UpdateMyProfileValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty()
            .WithMessage("Full name is required.")
            .MaximumLength(150)
            .WithMessage("Full name cannot exceed 150 characters.");

        RuleFor(x => x.Phone)
            .Must(PakistaniFormat.IsValidPhone)
            .WithMessage("Enter a valid Pakistani mobile number (e.g. 03001234567 or +923001234567).")
            .When(x => !string.IsNullOrWhiteSpace(x.Phone));

        RuleFor(x => x.Department)
            .MaximumLength(100)
            .WithMessage("Department cannot exceed 100 characters.");

        RuleFor(x => x.ProfileImage)
            .Must(file =>
            {
                if (file == null)
                    return true;

                return file.Length <= 5 * 1024 * 1024;
            })
            .WithMessage("Profile image cannot exceed 5 MB.")
            .Must(file =>
            {
                if (file == null)
                    return true;

                var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                return allowed.Contains(Path.GetExtension(file.FileName).ToLowerInvariant());
            })
            .WithMessage("Only JPG, JPEG, PNG, and WebP image formats are allowed.")

            .Must(file =>
            {
                if (file == null)
                    return true;

                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                using var stream = file.OpenReadStream();
                return LTSBackend.Comman.Security.FileSignatureValidator.HasValidSignature(stream, extension);
            })
            .WithMessage("File content does not match its image extension.");
    }
}