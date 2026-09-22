using FluentValidation;
using LTSBackend.Comman.Validation;

namespace LTSBackend.Features.Profile.Commands;

public class UpdateMyProfileValidator : AbstractValidator<UpdateMyProfileCommand>
{
    // Requires a full name; phone/department are optional but
    // format/length-checked. Phone goes through the same PakistaniFormat
    // check as registration/profile-completion (see that class's doc
    // comment: every write path MUST use it) - this used to be a looser,
    // one-off regex that accepted things like "0300-123-4567" or a phone
    // padded with spaces that IsValidPhone would reject, so a self-edit
    // could drift out of the canonical +923XXXXXXXXX format enforced
    // everywhere else. ProfileImage, if supplied, must be under 5MB and a
    // JPG/JPEG/PNG/WebP file.
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

                return file.Length <= 5 * 1024 * 1024;  // 5MB max
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
            // SECURITY: same content-signature check used on create/update-user's photo.
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