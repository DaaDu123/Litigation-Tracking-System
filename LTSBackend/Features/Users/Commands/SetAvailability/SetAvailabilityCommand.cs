using MediatR;

namespace LTSBackend.Features.Users.Commands.SetAvailability;

// Firm Admin sets their OWN availability. IsAvailable=true reactivates
// immediately (Reason/Days/Hours/Minutes are ignored). IsAvailable=false
// requires Reason and a strictly-positive total duration composed of any
// mix of Days/Hours/Minutes - validated on both frontend AND backend
// (SetAvailabilityValidator), per spec.
public record SetAvailabilityCommand(bool IsAvailable, string? Reason, int Days, int Hours, int Minutes) : IRequest<bool>;
