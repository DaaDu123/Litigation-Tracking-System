using MediatR;
namespace LTSBackend.Features.Users.Commands.ReleaseUserEmail;
public record ReleaseUserEmailCommand(int UserID) : IRequest<bool>
{
    public int ActingUserID { get; init; }
}
