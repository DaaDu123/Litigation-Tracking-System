using MediatR;
namespace LTSBackend.Features.Auth.ChangePassword
{
    public record ChangePasswordCommand(string OldPassword, string NewPassword) : IRequest<bool>
    {
        public int UserID { get; init; }
    }
}