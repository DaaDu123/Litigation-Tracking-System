using MediatR;

namespace LTSBackend.Features.Firms.Commands.UnblockFirm;

public record UnblockFirmCommand(int FirmID) : IRequest<bool>;
