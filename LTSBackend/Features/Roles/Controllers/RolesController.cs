using LTSBackend.Comman.Responses;
using LTSBackend.Features.Authorization;
using LTSBackend.Features.Roles.Commands.CreateRole;
using LTSBackend.Features.Roles.Commands.DeleteRole;
using LTSBackend.Features.Roles.Commands.UpdateRole;
using LTSBackend.Features.Roles.DTOs;
using LTSBackend.Features.Roles.Queries.GetAllRoles;
using LTSBackend.Features.Roles.Queries.GetRoleById;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LTSBackend.Features.Roles.Controllers;






















[Route("api/[controller]")]
[ApiController]
[HasPermission("ManageRoles")]
public class RolesController(IMediator mediator) : ControllerBase
{






    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var roles = await mediator.Send(new GetAllRolesQuery());

        return Ok(ApiResponse<List<RoleDTO>>.SuccessResponse(roles));
    }





    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var role = await mediator.Send(new GetRoleByIdQuery(id));
        return Ok(ApiResponse<RoleDTO>.SuccessResponse(role));
    }






    [HttpPost]
    public async Task<IActionResult> Create(CreateRoleCommand command)
    {
        var id = await mediator.Send(command);
        return Ok(ApiResponse<int>.SuccessResponse(id, "Role created successfully."));
    }






    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateRoleCommand command)
    {
        if (id != command.RoleID)
            return BadRequest();
        var result = await mediator.Send(command);
        return Ok(ApiResponse<bool>.SuccessResponse(result, "Role updated successfully."));
    }





    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await mediator.Send(new DeleteRoleCommand(id));
        return Ok(ApiResponse<bool>.SuccessResponse(result, "Role deleted successfully."));
    }
}
