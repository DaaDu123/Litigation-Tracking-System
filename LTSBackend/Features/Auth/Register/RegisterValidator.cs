using FluentValidation;

namespace LTSBackend.Features.Auth.Register;

public class RegisterValidator : AbstractValidator<RegisterCommand>
{
    // Firm User self-registration is Email + Password only now - full
    // name, phone, CNIC, and firm membership are all collected/chosen
    // later (profile completion, then a join request from the dashboard).
    public RegisterValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.")
            .MaximumLength(150).WithMessage("Email cannot exceed 150 characters.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one digit.")
            .Matches(@"[!@#$%^&*(),.?"":{}|<>_\-+=\[\]\\/;'~`]").WithMessage("Password must contain at least one symbol (!@#$%^&* etc.).");
    }
}
