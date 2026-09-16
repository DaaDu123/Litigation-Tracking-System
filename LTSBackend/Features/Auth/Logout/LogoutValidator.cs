using FluentValidation;
namespace LTSBackend.Features.Auth.Logout;

public class LogoutValidator : AbstractValidator<LogoutCommand>
{
    // No field-level rules — the refresh-token cookie itself is validated
    // inside LogoutHandler, not here.
    public LogoutValidator()
    {
        // Validation happens in handler — checks if refresh token exists in cookie
    }
}