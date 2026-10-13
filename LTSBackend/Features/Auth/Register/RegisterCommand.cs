using MediatR;
using LTSBackend.Comman.Behaviors;

namespace LTSBackend.Features.Auth.Register;






public record RegisterCommand(string Email, string Password) : IRequest<RegisterResponseDTO>, IAllowIncompleteProfile;
