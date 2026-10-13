using MediatR;

namespace LTSBackend.Features.Firms.Commands.DeleteFirm;


public record DeleteFirmCommand(int FirmID) : IRequest<bool>
{
    public int ActingUserID { get; init; }
}
