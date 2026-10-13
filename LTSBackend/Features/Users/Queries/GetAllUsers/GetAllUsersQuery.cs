using MediatR;
using LTSBackend.Features.Users.DTOs;
namespace LTSBackend.Features.Users.Queries.GetAllUsers;



public record GetAllUsersQuery(string? SearchTerm = null) : IRequest<List<UserDTO>>;
