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

// INTENTIONAL DESIGN (do not "fix" without reading this first):
// [HasPermission("ManageRoles")] gates this controller, but "ManageRoles"
// is deliberately NEVER seeded into the Permissions table or granted to
// any role in AppDbContext.SeedRolePermissions - see PermissionService,
// SuperAdmin implicitly holds every permission regardless of what's
// seeded, so this controller is reachable by SuperAdmin only, and by
// construction unreachable (403) for every other role.
// This is required, not accidental: Role and RolePermission are GLOBAL
// tables with no FirmID column - they are shared across every tenant in
// the system (see AppDbContext, neither entity has a HasQueryFilter). If
// "ManageRoles" were ever added to SeedRolePermissions for FirmAdmin (or
// any non-SuperAdmin role), that role would gain the ability to
// create/update/delete ANY firm's roles and rewrite ANY role's
// permission set platform-wide - a full cross-tenant privilege-escalation
// vulnerability, not merely a data leak. Per-firm role/permission
// customization, if ever needed, requires FirmID-scoped tables and a
// dedicated feature slice - it must NOT be achieved by granting
// "ManageRoles" more broadly here.
// Assigning EXISTING permissions to a user within one's own firm is a
// separate, already-scoped concern - see Features/Permissions (per-user
// grants) and CreateUser/ChangeUserRole's RoleHierarchy.CanAssignRole check.
[Route("api/[controller]")]
[ApiController]
[HasPermission("ManageRoles")]
public class RolesController(IMediator mediator) : ControllerBase
{
    // =====================================================
    // GET ALL ROLES — requires "ManageRoles" (SuperAdmin in practice)
    // Returns every role defined on the platform (SuperAdmin, FirmAdmin,
    // Partner, Associate, Moharrir, Intern, etc.), for the platform-level
    // role-management screen.
    // =====================================================
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var roles = await mediator.Send(new GetAllRolesQuery());

        return Ok(ApiResponse<List<RoleDTO>>.SuccessResponse(roles));
    }

    // =====================================================
    // GET ROLE BY ID — requires "ManageRoles" (SuperAdmin in practice)
    // Fetches a single role's details by its ID.
    // =====================================================
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var role = await mediator.Send(new GetRoleByIdQuery(id));
        return Ok(ApiResponse<RoleDTO>.SuccessResponse(role));
    }

    // =====================================================
    // CREATE ROLE — requires "ManageRoles" (SuperAdmin in practice)
    // Adds a brand-new, platform-wide role definition. See the
    // class-level note above for why this is intentionally SuperAdmin-only.
    // =====================================================
    [HttpPost]
    public async Task<IActionResult> Create(CreateRoleCommand command)
    {
        var id = await mediator.Send(command);
        return Ok(ApiResponse<int>.SuccessResponse(id, "Role created successfully."));
    }

    // =====================================================
    // UPDATE ROLE — requires "ManageRoles" (SuperAdmin in practice)
    // Edits an existing role's name/description. Route id and body
    // RoleID must match.
    // =====================================================
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateRoleCommand command)
    {
        if (id != command.RoleID)
            return BadRequest();
        var result = await mediator.Send(command);
        return Ok(ApiResponse<bool>.SuccessResponse(result, "Role updated successfully."));
    }

    // =====================================================
    // DELETE ROLE — requires "ManageRoles" (SuperAdmin in practice)
    // Removes a platform-wide role definition.
    // =====================================================
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await mediator.Send(new DeleteRoleCommand(id));
        return Ok(ApiResponse<bool>.SuccessResponse(result, "Role deleted successfully."));
    }
}
