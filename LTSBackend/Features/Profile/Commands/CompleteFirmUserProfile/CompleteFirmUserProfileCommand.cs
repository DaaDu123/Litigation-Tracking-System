using LTSBackend.Comman.Behaviors;
using LTSBackend.Features.Profile.DTOs;
using MediatR;

namespace LTSBackend.Features.Profile.Commands.CompleteFirmUserProfile;

// Mandatory step for a Firm User after registration (Email+Password) and
// before they can browse/request a firm. Implements IAllowIncompleteProfile
// since this is the one command an incomplete-profile Firm User IS
// allowed to run.
public record CompleteFirmUserProfileCommand(
    string FullName,
    string Phone,
    string CNIC,
    string? ProfileImageUrl
) : IRequest<ProfileCompletionResultDTO>, IAllowIncompleteProfile;
