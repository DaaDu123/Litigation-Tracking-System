using LTSBackend.Comman.Behaviors;
using LTSBackend.Features.Profile.DTOs;
using MediatR;

namespace LTSBackend.Features.Profile.Commands.CompleteFirmAdminProfile;

// Mandatory step immediately after a Firm Admin's first successful login.
// Replaces the system-generated placeholders (FullName, Firm.FirmName)
// set at approval time and captures the required identity details
// (Phone, CNIC) subject to the same Pakistan-format + uniqueness rules
// as everyone else. Implements IAllowIncompleteProfile because this is
// the one command an incomplete-profile Firm Admin IS allowed to run.
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
