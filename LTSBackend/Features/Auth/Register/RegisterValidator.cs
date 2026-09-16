using FluentValidation;

namespace LTSBackend.Features.Auth.Register;

public class RegisterValidator : AbstractValidator<RegisterCommand>
{
    // Validates the self-registration form: name/email/password-complexity/
    // optional phone format, and requires a FirmCode (the firm's own
    // registration code) so the account can be scoped to the right firm.
    public RegisterValidator()
    {
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

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required.")
            .MinimumLength(8)
            .WithMessage("Password must be at least 8 characters long.")
            .Matches("[A-Z]")
            .WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[a-z]")
            .WithMessage("Password must contain at least one lowercase letter.")
            .Matches("[0-9]")
            .WithMessage("Password must contain at least one digit.")
            .Matches(@"[!@#$%^&*(),.?"":{}|<>_\-+=\[\]\\/;'~`]")
            .WithMessage("Password must contain at least one symbol (!@#$%^&* etc.).");

        RuleFor(x => x.Phone)
            .MaximumLength(20)
            .WithMessage("Phone cannot exceed 20 characters.")
            .Matches(@"^\+?[0-9\-\(\)\s]*$")
            .WithMessage("Phone format is invalid.")
            .When(x => !string.IsNullOrWhiteSpace(x.Phone));

        RuleFor(x => x.Department)
            .MaximumLength(100)
            .WithMessage("Department cannot exceed 100 characters.");

        RuleFor(x => x.FirmCode)
            .NotEmpty()
            .WithMessage("Firm code is required - ask your Firm Admin for your firm's registration code.");
    }
}