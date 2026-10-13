using FluentValidation;
namespace LTSBackend.Features.Auth.VerifyOtp;

public class VerifyOtpValidator : AbstractValidator<VerifyOtpCommand>
{

    public VerifyOtpValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required.")
            .EmailAddress()
            .WithMessage("Invalid email format.");

        RuleFor(x => x.OtpCode)
            .NotEmpty()
            .WithMessage("OTP code is required.")
            .Length(6)
            .WithMessage("OTP must be exactly 6 digits.");
    }
}