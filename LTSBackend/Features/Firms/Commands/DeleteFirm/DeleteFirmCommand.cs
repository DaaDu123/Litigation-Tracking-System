using MediatR;

namespace LTSBackend.Features.Firms.Commands.DeleteFirm;

/// <summary>Soft-removes a firm workspace. Does NOT physically delete data - use the export endpoint first if the firm needs its data handed back.</summary>
public record DeleteFirmCommand(int FirmID) : IRequest<bool>
{
    public int ActingUserID { get; init; }
}
