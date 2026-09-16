using FluentValidation;

namespace LTSBackend.Features.Auth.RefreshToken;

public class RefreshTokenValidator : AbstractValidator<RefreshTokenCommand>
{
    // No field-level rules — the request carries no body fields; the
    // refresh-token cookie itself is validated inside RefreshTokenHandler.
    public RefreshTokenValidator()
    {

    }
}