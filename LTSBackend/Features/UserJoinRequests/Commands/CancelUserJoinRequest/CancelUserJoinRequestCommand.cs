using MediatR;

namespace LTSBackend.Features.UserJoinRequests.Commands.CancelUserJoinRequest;



public record CancelUserJoinRequestCommand(int RequestID) : IRequest<bool>;
