using LTSBackend.Comman.Behaviors;
using LTSBackend.Features.Users.DTOs;
using MediatR;

namespace LTSBackend.Features.Users.Queries.GetFirmAdminAvailability;








public record GetFirmAdminAvailabilityQuery : IRequest<FirmAdminAvailabilityDTO>, IAllowIncompleteProfile;
