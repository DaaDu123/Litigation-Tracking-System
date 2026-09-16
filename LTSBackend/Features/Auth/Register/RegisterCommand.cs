using MediatR;
namespace LTSBackend.Features.Auth.Register;

public record RegisterCommand(string FullName, string Email, string Password, string? Phone, string? Department, string FirmCode) : IRequest<RegisterResponseDTO>;
