using MediatR;
using LTSBackend.Comman.Behaviors;

namespace LTSBackend.Features.FirmAdminRequests.Commands.SubmitFirmAdminRequest;

// Firm Admin self-registration is now Email + Password only (no FirmCode,
// no firm details, no personal details) - everything else is collected
// later, during the mandatory profile-completion step that follows
// SuperAdmin approval (CompleteFirmAdminProfileCommand).
public record SubmitFirmAdminRequestCommand(string Email, string Password) : IRequest<int>, IAllowIncompleteProfile;
