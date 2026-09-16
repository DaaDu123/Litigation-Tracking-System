using MediatR;
namespace LTSBackend.Features.Users.Commands.ActivateUser;
public record ActivateUserCommand(int UserID) : IRequest<bool>
{
    public int ActingUserID { get; init; }
}
