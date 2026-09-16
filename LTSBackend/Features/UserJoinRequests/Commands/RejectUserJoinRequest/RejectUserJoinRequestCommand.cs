using MediatR;

namespace LTSBackend.Features.UserJoinRequests.Commands.RejectUserJoinRequest;

public record RejectUserJoinRequestCommand(int RequestID, string? Reason) : IRequest<bool>
{
    public int ActingUserID { get; init; }
}
