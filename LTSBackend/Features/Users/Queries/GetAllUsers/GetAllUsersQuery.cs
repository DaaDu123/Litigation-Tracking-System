using MediatR;
using LTSBackend.Features.Users.DTOs;
namespace LTSBackend.Features.Users.Queries.GetAllUsers;

// SearchTerm matches Name or Contact Number (server-side, per spec) -
// null/empty returns the full firm directory as before.
public record GetAllUsersQuery(string? SearchTerm = null) : IRequest<List<UserDTO>>;
