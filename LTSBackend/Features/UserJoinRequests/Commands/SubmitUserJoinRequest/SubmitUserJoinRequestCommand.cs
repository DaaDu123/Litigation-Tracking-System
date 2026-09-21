using MediatR;

namespace LTSBackend.Features.UserJoinRequests.Commands.SubmitUserJoinRequest;

// Submitted by an already-registered, already-profile-completed Firm User
// from their own dashboard - registration (email+password) and the firm
// request are now two separate steps. No personal details are collected
// here anymore (the user already has an account); the accepted role is
// always InternParalegal by default per business rule, so it isn't
// requested here either - the Firm Admin changes it afterward if needed.
public record SubmitUserJoinRequestCommand(int FirmID) : IRequest<int>;
