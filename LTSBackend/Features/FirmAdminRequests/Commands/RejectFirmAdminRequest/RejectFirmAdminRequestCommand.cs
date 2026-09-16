using MediatR;

namespace LTSBackend.Features.FirmAdminRequests.Commands.RejectFirmAdminRequest;
public record RejectFirmAdminRequestCommand(int RequestID, string? Reason) : IRequest<bool>
{
    public int ActingUserID { get; init; }
}
