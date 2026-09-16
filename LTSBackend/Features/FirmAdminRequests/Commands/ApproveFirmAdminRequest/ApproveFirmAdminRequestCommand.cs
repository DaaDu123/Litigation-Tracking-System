using MediatR;

namespace LTSBackend.Features.FirmAdminRequests.Commands.ApproveFirmAdminRequest;
public record ApproveFirmAdminRequestCommand(int RequestID) : IRequest<int>
{
    public int ActingUserID { get; init; }
}
