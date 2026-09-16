using LTSBackend.Features.UserJoinRequests.DTOs;
using MediatR;

namespace LTSBackend.Features.UserJoinRequests.Queries.GetUserJoinRequests;

public record GetUserJoinRequestsQuery(string? Status) : IRequest<List<UserJoinRequestDTO>>;
