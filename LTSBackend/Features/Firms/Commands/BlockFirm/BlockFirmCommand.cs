using MediatR;

namespace LTSBackend.Features.Firms.Commands.BlockFirm;

/// <summary>Blocking a firm immediately stops every user under it from logging in (see LoginHandler).</summary>
public record BlockFirmCommand(int FirmID, string? Reason) : IRequest<bool>
{
    public int ActingUserID { get; init; }
}
