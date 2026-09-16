using MediatR;
namespace LTSBackend.Features.Users.Commands.PermanentDeleteUser;
public record PermanentDeleteUserCommand(int UserID) : IRequest<bool>
{
    public int ActingUserID { get; init; }
}
