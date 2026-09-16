using MediatR;
using LTSBackend.Features.Users.DTOs;
namespace LTSBackend.Features.Users.Queries.GetDeletedUsers;

public record GetDeletedUsersQuery() : IRequest<List<DeletedUserDTO>>;
