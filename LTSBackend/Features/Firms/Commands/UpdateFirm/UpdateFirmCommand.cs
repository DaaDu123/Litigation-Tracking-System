using MediatR;

namespace LTSBackend.Features.Firms.Commands.UpdateFirm;

public record UpdateFirmCommand(
    int FirmID,
    string FirmName,
    string? Address,
    string? ContactEmail,
    string? ContactPhone,
    string? CustomDomain
) : IRequest<bool>;
