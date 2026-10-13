using MediatR;

namespace LTSBackend.Features.Users.Commands.SetAvailability;






public record SetAvailabilityCommand(bool IsAvailable, string? Reason, int Days, int Hours, int Minutes) : IRequest<bool>;
