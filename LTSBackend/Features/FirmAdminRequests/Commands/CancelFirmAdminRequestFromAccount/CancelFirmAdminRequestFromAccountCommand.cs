using MediatR;

namespace LTSBackend.Features.FirmAdminRequests.Commands.CancelFirmAdminRequestFromAccount;

public record CancelFirmAdminRequestFromAccountCommand(int RequestID) : IRequest<bool>;
