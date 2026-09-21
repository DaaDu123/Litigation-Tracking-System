using LTSBackend.Comman.Behaviors;
using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using LTSBackend.Features.Profile.DTOs;
using MediatR;
namespace LTSBackend.Features.Profile.Queries;

// Marked IAllowIncompleteProfile: the frontend needs this endpoint to
// work even before profile completion, to know IsProfileCompleted/
// CNIC/Phone in the first place and decide whether to show the
// mandatory completion modal.
public record GetMyProfileQuery(int UserID) : IRequest<ProfileDTO>, IAllowIncompleteProfile;
