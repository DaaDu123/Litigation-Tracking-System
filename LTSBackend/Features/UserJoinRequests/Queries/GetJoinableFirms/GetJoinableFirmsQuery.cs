using LTSBackend.Features.UserJoinRequests.DTOs;
using MediatR;

namespace LTSBackend.Features.UserJoinRequests.Queries.GetJoinableFirms;

public record GetJoinableFirmsQuery : IRequest<List<JoinableFirmDTO>>;
