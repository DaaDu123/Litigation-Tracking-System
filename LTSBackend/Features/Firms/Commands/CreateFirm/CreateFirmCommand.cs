using MediatR;

namespace LTSBackend.Features.Firms.Commands.CreateFirm;









public record CreateFirmCommand(
    string FirmName,
    string? Address,
    string? ContactEmail,
    string? ContactPhone,
    string AdminFullName,
    string AdminEmail,
    string AdminPassword
) : IRequest<int>
{
    public int ActingUserID { get; init; }
}
