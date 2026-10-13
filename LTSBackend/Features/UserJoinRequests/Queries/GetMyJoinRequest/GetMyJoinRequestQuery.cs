using LTSBackend.Features.UserJoinRequests.DTOs;
using MediatR;

namespace LTSBackend.Features.UserJoinRequests.Queries.GetMyJoinRequest;




public record GetMyJoinRequestQuery : IRequest<UserJoinRequestDTO?>;
