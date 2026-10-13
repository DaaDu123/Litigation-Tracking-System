using LTSBackend.Comman.Responses;
using LTSBackend.Features.Authorization;
using LTSBackend.Features.Permissions.Commands.AssignPermissions;
using LTSBackend.Features.Permissions.DTOs;
using LTSBackend.Features.Permissions.Queries.GetAllPermissions;
using LTSBackend.Features.Permissions.Queries.GetRolePermissions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LTSBackend.Features.Permissions.Controllers;









[Route("api/[controller]")]
[ApiController]
[HasPermission("ManageRoles")]
public class PermissionsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<PermissionsController> _logger;

    public PermissionsController(IMediator mediator, ILogger<PermissionsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }






    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        _logger.LogInformation("Get all permissions request");

        var permissions = await _mediator.Send(new GetAllPermissionsQuery());

        return Ok(ApiResponse<List<PermissionDTO>>.SuccessResponse(permissions,"Permissions fetched successfully."));
    }






    [HttpGet("role/{roleId}")]
    public async Task<IActionResult> GetRolePermissions(int roleId)
    {
        _logger.LogInformation("Get role permissions request: {RoleID}", roleId);

        var permissions = await _mediator.Send(new GetRolePermissionsQuery(roleId));

        return Ok(ApiResponse<List<PermissionDTO>>.SuccessResponse(permissions,"Role permissions fetched successfully."));
    }








    [HttpPut("assign")]
    public async Task<IActionResult> AssignPermissions([FromBody] AssignPermissionsCommand command)
    {
        _logger.LogInformation("Assign permissions request for role: {RoleID}",command.RoleID);

        var result = await _mediator.Send(command);

        return Ok(ApiResponse<bool>.SuccessResponse(result,"Permissions assigned successfully."));
    }
}
