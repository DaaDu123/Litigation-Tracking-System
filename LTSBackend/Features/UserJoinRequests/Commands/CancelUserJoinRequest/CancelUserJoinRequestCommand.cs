using MediatR;

namespace LTSBackend.Features.UserJoinRequests.Commands.CancelUserJoinRequest;

// The requester cancels their own still-pending join request, freeing
// them to request a different firm immediately afterward.
public record CancelUserJoinRequestCommand(int RequestID) : IRequest<bool>;
