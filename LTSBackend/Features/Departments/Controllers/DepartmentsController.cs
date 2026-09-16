using LTSBackend.Comman.Responses;
using LTSBackend.Features.Departments.Commands.CreateDepartment;
using LTSBackend.Features.Departments.Commands.DeleteDepartment;
using LTSBackend.Features.Departments.Commands.UpdateDepartment;
using LTSBackend.Features.Departments.DTOs;
using LTSBackend.Features.Departments.Queries.GetAllDepartments;
using LTSBackend.Features.Departments.Queries.GetDepartmentById;
using LTSBackend.Models.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LTSBackend.Features.Departments.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class DepartmentsController(IMediator mediator) : ControllerBase
{
    // =====================================================
    // GET ALL DEPARTMENTS — FirmAdmin and above ONLY
    // Master-data management is FirmAdmin and Partner's task.
    // AssociateLawyer, Moharrir, InternParalegal and SuperAdmin have NO
    // access (not even read) to this master data admin surface. Query
    // results are automatically scoped by the caller's visibility
    // (system-wide global departments + their own firm's custom ones) via
    // the HasQueryFilter on Department in AppDbContext.
    // =====================================================
    [HttpGet]
    [Authorize(Roles = RoleNames.FirmAdminAndAbove)]
    public async Task<IActionResult> GetAll([FromQuery] bool activeOnly = false)
    {
        var departments = await mediator.Send(new GetAllDepartmentsQuery(activeOnly));
        return Ok(ApiResponse<List<DepartmentDTO>>.SuccessResponse(departments));
    }

    // =====================================================
    // GET DEPARTMENT BY ID — FirmAdmin and above ONLY
    // Fetches a single department's details by its ID.
    // =====================================================
    [HttpGet("{id}")]
    [Authorize(Roles = RoleNames.FirmAdminAndAbove)]
    public async Task<IActionResult> GetById(int id)
    {
        var department = await mediator.Send(new GetDepartmentByIdQuery(id));
        return Ok(ApiResponse<DepartmentDTO>.SuccessResponse(department));
    }

    // =====================================================
    // CREATE DEPARTMENT — FirmAdmin and above
    // Adds a new department. Same per-tenant model as Court — FirmID is
    // nullable (NULL = system-wide global department; a real value = a
    // firm's own custom department). CreateDepartmentHandler assigns
    // ownership on create. Requires the EF migration that adds
    // Department.FirmID to be applied.
    // =====================================================
    [HttpPost]
    [Authorize(Roles = RoleNames.FirmAdminAndAbove)]
    public async Task<IActionResult> Create(CreateDepartmentCommand command)
    {
        var id = await mediator.Send(command);
        return Ok(ApiResponse<int>.SuccessResponse(id, "Department created successfully."));
    }

    // =====================================================
    // UPDATE DEPARTMENT — FirmAdmin and above
    // Edits an existing department. A FirmAdmin may only update their
    // OWN firm's custom department (enforced in UpdateDepartmentHandler)
    // — never a global or another firm's department.
    // =====================================================
    [HttpPut("{id}")]
    [Authorize(Roles = RoleNames.FirmAdminAndAbove)]
    public async Task<IActionResult> Update(int id, UpdateDepartmentCommand command)
    {
        if (id != command.DepartmentID)
            return BadRequest(ApiResponse<bool>.FailureResponse("Route ID and body DepartmentID do not match."));

        var result = await mediator.Send(command);
        return Ok(ApiResponse<bool>.SuccessResponse(result, "Department updated successfully."));
    }

    // =====================================================
    // DELETE DEPARTMENT — FirmAdmin and above
    // Removes a department the firm no longer needs. A FirmAdmin may only
    // delete their OWN firm's custom department (enforced in
    // DeleteDepartmentHandler) — never a global or another firm's
    // department.
    // =====================================================
    [HttpDelete("{id}")]
    [Authorize(Roles = RoleNames.FirmAdminAndAbove)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await mediator.Send(new DeleteDepartmentCommand(id));
        return Ok(ApiResponse<bool>.SuccessResponse(result, "Department deleted successfully."));
    }
}
