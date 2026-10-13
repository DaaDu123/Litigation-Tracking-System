using LTSBackend.Comman.Behaviors;
using LTSBackend.Features.Profile.DTOs;
using MediatR;

namespace LTSBackend.Features.Profile.Commands.CompleteFirmUserProfile;





public record CompleteFirmUserProfileCommand(
    string FullName,
    string Phone,
    string CNIC,
    string? ProfileImageUrl
) : IRequest<ProfileCompletionResultDTO>, IAllowIncompleteProfile;
