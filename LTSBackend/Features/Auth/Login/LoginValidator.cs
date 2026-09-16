using FluentValidation;
namespace LTSBackend.Features.Auth.Login;

public class LoginValidator : AbstractValidator<LoginCommand>
{
    // Requires a well-formed email and a non-empty password.
    public LoginValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required.")
            .EmailAddress()
            .WithMessage("Invalid email format.");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required.");
    }
}