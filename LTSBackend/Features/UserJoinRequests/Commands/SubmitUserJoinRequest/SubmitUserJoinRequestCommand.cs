using MediatR;

namespace LTSBackend.Features.UserJoinRequests.Commands.SubmitUserJoinRequest;

public record SubmitUserJoinRequestCommand(
    int FirmID,
    string FullName,
    string Email,
    string Password,
    string? Phone,
    string? Department,
    int RequestedRoleID) : IRequest<int>;
