using MediatR;
using LTSBackend.Comman.Behaviors;

namespace LTSBackend.Features.Auth.Register;

// Firm User self-registration: Email + Password only. No FirmCode, no
// firm selection, no personal details - those are collected afterward
// during mandatory profile completion (CompleteFirmUserProfileCommand),
// and the firm itself is chosen even later, via a UserJoinRequest sent
// from the Firm User's own dashboard.
public record RegisterCommand(string Email, string Password) : IRequest<RegisterResponseDTO>, IAllowIncompleteProfile;
