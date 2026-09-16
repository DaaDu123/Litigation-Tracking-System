using FluentValidation;
namespace LTSBackend.Features.Auth.ResendOtp;

public class ResendOtpValidator : AbstractValidator<ResendOtpCommand>
{
    // Requires a well-formed email address.
    public ResendOtpValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required.")
            .EmailAddress()
            .WithMessage("Invalid email format.");
    }
}