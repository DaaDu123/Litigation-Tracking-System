using FluentValidation;
using MediatR;

namespace LTSBackend.Features.Auth.Login;

public record LoginCommand(string Email, string Password) : IRequest<LoginResponseDTO>;