using MediatR;

namespace LTSBackend.Features.Firms.Commands.BlockFirm;


public record BlockFirmCommand(int FirmID, string? Reason) : IRequest<bool>
{
    public int ActingUserID { get; init; }
}
