using LTSBackend.Features.Roles.DTOs;
using MediatR;
namespace LTSBackend.Features.Roles.Queries.GetRoleById;

public record GetRoleByIdQuery(int RoleID) : IRequest<RoleDTO>;