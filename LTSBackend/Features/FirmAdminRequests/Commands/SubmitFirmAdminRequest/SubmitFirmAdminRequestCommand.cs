using MediatR;

namespace LTSBackend.Features.FirmAdminRequests.Commands.SubmitFirmAdminRequest;

public record SubmitFirmAdminRequestCommand(string FirmName,string FirmCode,string? Address,string? ContactEmail,string? ContactPhone,string AdminFullName,string AdminEmail,string AdminPassword,string? AdminPhone) : IRequest<int>;
