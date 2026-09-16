using FluentValidation;
namespace LTSBackend.Features.Profile.Commands;

public class UpdateMyProfileValidator : AbstractValidator<UpdateMyProfileCommand>
{
    // Requires a full name; phone/department are optional but
    // format/length-checked. ProfileImage, if supplied, must be under 5MB
    // and a JPG/JPEG/PNG/WebP file.
    public UpdateMyProfileValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty()
            .WithMessage("Full name is required.")
            .MaximumLength(150)
            .WithMessage("Full name cannot exceed 150 characters.");

        RuleFor(x => x.Phone)
            .MaximumLength(20)
            .WithMessage("Phone cannot exceed 20 characters.")
            .Matches(@"^\+?[0-9\-\(\)\s]*$")
            .WithMessage("Phone format is invalid.")
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
            .WithMessage("Only JPG, JPEG, PNG, and WebP image formats are allowed.");
    }
}