using MediatR;
using LTSBackend.Features.Users.DTOs;
namespace LTSBackend.Features.Users.Queries.GetUserById;

public record GetUserByIdQuery(int UserID) : IRequest<UserDTO?>;