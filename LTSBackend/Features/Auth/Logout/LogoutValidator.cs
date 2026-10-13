using FluentValidation;
namespace LTSBackend.Features.Auth.Logout;

public class LogoutValidator : AbstractValidator<LogoutCommand>
{


    public LogoutValidator()
    {

    }
}