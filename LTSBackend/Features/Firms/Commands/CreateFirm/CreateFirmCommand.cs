using MediatR;

namespace LTSBackend.Features.Firms.Commands.CreateFirm;

/// <summary>
/// SuperAdmin-only: provisions a brand-new firm workspace and, in the
/// same operation, bootstraps its first Firm Admin account (a firm
/// can't create its own users until it has at least one admin).
/// </summary>
public record CreateFirmCommand(
    string FirmName,
    string FirmCode,
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
