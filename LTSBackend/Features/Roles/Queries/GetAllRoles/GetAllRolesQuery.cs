using LTSBackend.Features.Roles.DTOs;
using MediatR;
namespace LTSBackend.Features.Roles.Queries.GetAllRoles;

public record GetAllRolesQuery : IRequest<List<RoleDTO>>;