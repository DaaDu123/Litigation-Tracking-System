using MediatR;

namespace LTSBackend.Features.UserJoinRequests.Commands.SubmitUserJoinRequest;







public record SubmitUserJoinRequestCommand(int FirmID) : IRequest<int>;
