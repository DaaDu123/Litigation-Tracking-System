using LTSBackend.Features.UserJoinRequests.DTOs;
using MediatR;

namespace LTSBackend.Features.UserJoinRequests.Queries.GetMyJoinRequest;

// The requester's own latest join request (any status), for their "Request
// Pending / Cancel Request" dashboard card. Returns null if they've never
// submitted one.
public record GetMyJoinRequestQuery : IRequest<UserJoinRequestDTO?>;
