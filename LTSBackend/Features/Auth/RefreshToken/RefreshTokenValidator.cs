using FluentValidation;

namespace LTSBackend.Features.Auth.RefreshToken;

public class RefreshTokenValidator : AbstractValidator<RefreshTokenCommand>
{


    public RefreshTokenValidator()
    {

    }
}