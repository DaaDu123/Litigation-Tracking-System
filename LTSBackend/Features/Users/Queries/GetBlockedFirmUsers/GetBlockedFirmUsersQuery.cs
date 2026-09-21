using LTSBackend.Features.Users.DTOs;
using MediatR;

namespace LTSBackend.Features.Users.Queries.GetBlockedFirmUsers;

public record GetBlockedFirmUsersQuery : IRequest<List<BlockedFirmUserDTO>>;
