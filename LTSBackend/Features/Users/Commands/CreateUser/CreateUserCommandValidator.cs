using FluentValidation;
using LTSBackend.Comman.Enum;

namespace LTSBackend.Features.Users.Commands.CreateUser;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    // Requires name/email/password (standard complexity rules)/a defined
    // RoleID; phone/department are optional but format/length-checked.
    // ProfileImage, if supplied, must be under 5MB, a JPG/JPEG/PNG/WebP
    // file, AND have a byte signature matching that extension (blocks a
    // renamed/mislabeled file).
    public CreateUserCommandValidator()
    {
        // Full Name is a required field in the Add User form — the
        // backend no longer guesses/derives it.
        RuleFor(x => x.FullName)
            .NotEmpty()
            .WithMessage("Full name is required")
            .MaximumLength(150)
            .WithMessage("Full name cannot exceed 150 characters");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .WithMessage("Invalid email format")
            .MaximumLength(150)
            .WithMessage("Email cannot exceed 150 characters");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required")
            .MinimumLength(8)
            .WithMessage("Password must be at least 8 characters")
            .Matches("[A-Z]")
            .WithMessage("Password must contain at least one uppercase letter")
            .Matches("[a-z]")
            .WithMessage("Password must contain at least one lowercase letter")
            .Matches("[0-9]")
            .WithMessage("Password must contain at least one digit")
            .Matches(@"[!@#$%^&*(),.?"":{}|<>_\-+=\[\]\\/;'~`]")
            .WithMessage("Password must contain at least one symbol (!@#$%^&* etc.)");

        RuleFor(x => x.Phone)
            .MaximumLength(20)
            .WithMessage("Phone cannot exceed 20 characters")
            .Matches(@"^\+?[0-9\-\(\)\s]*$")
            .WithMessage("Phone format is invalid")
            .When(x => !string.IsNullOrWhiteSpace(x.Phone));

        RuleFor(x => x.Department)
            .MaximumLength(100)
            .WithMessage("Department cannot exceed 100 characters");

        // Optional on create — omitted means "default to Intern/Paralegal"
        // (handled in CreateUserCommandHandler). If a role IS supplied it
        // must still be a real, valid role ID.
        RuleFor(x => x.RoleID)
            .GreaterThan(0)
            .WithMessage("Valid role is required")
            .Must(roleId => roleId.HasValue && Enum.IsDefined(typeof(UserRole), roleId.Value))
            .WithMessage("Invalid role")
            .When(x => x.RoleID.HasValue);

        RuleFor(x => x.ProfileImage)
            .Must(file =>
            {
                if (file == null)
                    return true;

                return file.Length <= 5 * 1024 * 1024; // 5MB max
            })
            .WithMessage("Profile image cannot exceed 5 MB")
            .Must(file =>
            {
                if (file == null)
                    return true;

                var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                return allowed.Contains(Path.GetExtension(file.FileName).ToLowerInvariant());
            })
            .WithMessage("Only JPG, JPEG, PNG, and WebP formats are allowed")
            // SECURITY: same content-signature check as case document
            // uploads (see UploadDocumentValidator) - rejects a file whose
            // actual bytes don't match its claimed image extension.
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