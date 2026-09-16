using FluentValidation;
using LTSBackend.Comman.Enum;
using LTSBackend.Models;

namespace LTSBackend.Features.Users.Commands.UpdateUser;

public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    // Requires a valid UserID plus the same name/email/role/phone/
    // department/profile-image rules as CreateUserCommandValidator (minus
    // the password, which isn't editable through this command).
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.UserID)
            .GreaterThan(0)
            .WithMessage("Valid user ID is required.");

        RuleFor(x => x.FullName)
            .NotEmpty()
            .WithMessage("Full name is required.")
            .MaximumLength(150)
            .WithMessage("Full name cannot exceed 150 characters.");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required.")
            .EmailAddress()
            .WithMessage("Invalid email format.")
            .MaximumLength(150)
            .WithMessage("Email cannot exceed 150 characters.");

        RuleFor(x => x.Phone)
            .MaximumLength(20)
            .WithMessage("Phone cannot exceed 20 characters.")
            .Matches(@"^\+?[0-9\-\(\)\s]*$")
            .WithMessage("Phone format is invalid.")
            .When(x => !string.IsNullOrWhiteSpace(x.Phone));

        RuleFor(x => x.Department)
            .MaximumLength(100)
            .WithMessage("Department cannot exceed 100 characters.");

        RuleFor(x => x.RoleID)
            .NotNull()
            .WithMessage("Role is required.")
            .Must(roleId => roleId.HasValue && Enum.IsDefined(typeof(UserRole), roleId.Value))
            .WithMessage("Invalid role. Must be: SuperAdmin(1), FirmAdmin(2), Partner(3), AssociateLawyer(4), Moharrir(5), or InternParalegal(6).");

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
            // SECURITY: same content-signature check as CreateUserCommandValidator.
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
