using LTSBackend.Comman.Behaviors;
using LTSBackend.Features.Profile.DTOs;
using MediatR;

namespace LTSBackend.Features.Profile.Commands.CompleteFirmAdminProfile;







public record CompleteFirmAdminProfileCommand(
    string FullName,
    string Phone,
    string CNIC,
    string FirmName,
    string? Address,
    string? ContactEmail,
    string? ContactPhone,
    string? ProfileImageUrl
) : IRequest<ProfileCompletionResultDTO>, IAllowIncompleteProfile;
