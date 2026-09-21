using LTSBackend.Comman.Behaviors;
using LTSBackend.Features.Users.DTOs;
using MediatR;

namespace LTSBackend.Features.Users.Queries.GetFirmAdminAvailability;

// Returns the CALLING user's own firm's FirmAdmin availability status -
// there's no FirmAdminUserID parameter because a Firm User should only
// ever be able to see their OWN firm's admin, never pick an arbitrary ID.
// Marked IAllowIncompleteProfile: the topbar availability widget calls
// this immediately after login, before profile completion is done (a
// FirmAdmin already has FirmID set at that point, just not IsProfileCompleted
// yet) - this is a harmless, read-only status check, not a reason to block.
public record GetFirmAdminAvailabilityQuery : IRequest<FirmAdminAvailabilityDTO>, IAllowIncompleteProfile;
