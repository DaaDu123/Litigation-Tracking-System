using MediatR;

namespace LTSBackend.Features.UserJoinRequests.Commands.ApproveUserJoinRequest;

public record ApproveUserJoinRequestCommand(int RequestID) : IRequest<int>
{
    public int ActingUserID { get; init; }
}
