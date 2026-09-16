using LTSBackend.Comman.Responses;
using LTSBackend.Features.Authorization;
using LTSBackend.Features.Permissions.Commands.AssignPermissions;
using LTSBackend.Features.Permissions.DTOs;
using LTSBackend.Features.Permissions.Queries.GetAllPermissions;
using LTSBackend.Features.Permissions.Queries.GetRolePermissions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LTSBackend.Features.Permissions.Controllers;

// INTENTIONAL DESIGN - see the identical note on RolesController.
// [HasPermission("ManageRoles")] is deliberately unreachable by any role
// except SuperAdmin (who bypasses permission checks entirely) because
// Permission/RolePermission are GLOBAL, non-tenant-scoped tables.
// AssignPermissions here rewrites a Role's ENTIRE permission set
// platform-wide; do not grant "ManageRoles" to FirmAdmin or any other
// role without first adding tenant scoping to Role/RolePermission -
// otherwise this becomes a cross-tenant privilege-escalation path.
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

    // =====================================================
    // GET ALL PERMISSIONS — requires "ManageRoles" (SuperAdmin in practice)
    // Returns every permission defined on the platform, for populating the
    // role/permission-assignment screen.
    // =====================================================
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        _logger.LogInformation("Get all permissions request");

        var permissions = await _mediator.Send(new GetAllPermissionsQuery());

        return Ok(ApiResponse<List<PermissionDTO>>.SuccessResponse(permissions,"Permissions fetched successfully."));
    }

    // =====================================================
    // GET ROLE PERMISSIONS — requires "ManageRoles" (SuperAdmin in practice)
    // Returns the permissions currently granted to a specific role, so the
    // assignment screen can show what's checked/unchecked.
    // =====================================================
    [HttpGet("role/{roleId}")]
    public async Task<IActionResult> GetRolePermissions(int roleId)
    {
        _logger.LogInformation("Get role permissions request: {RoleID}", roleId);

        var permissions = await _mediator.Send(new GetRolePermissionsQuery(roleId));

        return Ok(ApiResponse<List<PermissionDTO>>.SuccessResponse(permissions,"Role permissions fetched successfully."));
    }

    // =====================================================
    // ASSIGN PERMISSIONS TO ROLE — requires "ManageRoles" (SuperAdmin in practice)
    // Replaces a role's ENTIRE permission set platform-wide with the
    // supplied list. See the class-level note above — this is a global,
    // non-tenant-scoped operation, which is exactly why it's locked to
    // SuperAdmin today.
    // =====================================================
    [HttpPut("assign")]
    public async Task<IActionResult> AssignPermissions([FromBody] AssignPermissionsCommand command)
    {
        _logger.LogInformation("Assign permissions request for role: {RoleID}",command.RoleID);

        var result = await _mediator.Send(command);

        return Ok(ApiResponse<bool>.SuccessResponse(result,"Permissions assigned successfully."));
    }
}
