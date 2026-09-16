using LTSBackend.Comman.Responses;
using LTSBackend.Features.CaseStatuses.Commands.CreateCaseStatus;
using LTSBackend.Features.CaseStatuses.Commands.DeleteCaseStatus;
using LTSBackend.Features.CaseStatuses.Commands.UpdateCaseStatus;
using LTSBackend.Features.CaseStatuses.DTOs;
using LTSBackend.Features.CaseStatuses.Queries.GetAllCaseStatuses;
using LTSBackend.Features.CaseStatuses.Queries.GetCaseStatusById;
using LTSBackend.Models.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LTSBackend.Features.CaseStatuses.Controllers;

/// <summary>
/// Master data for case workflow statuses (e.g. Filed, Under Trial, Closed).
/// Same per-tenant model as Courts/Departments/CaseCategories/CaseStages/
/// DocumentTypes - see CreateCaseStatusHandler/UpdateCaseStatusHandler/
/// DeleteCaseStatusHandler for the ownership rules.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class CaseStatusesController(IMediator mediator) : ControllerBase
{
    // =====================================================
    // GET ALL CASE STATUSES — FirmAdmin and above ONLY
    // Master-data management is FirmAdmin and Partner's task.
    // AssociateLawyer, Moharrir, InternParalegal and SuperAdmin have NO
    // access (not even read) to this master data admin surface.
    // =====================================================
    [HttpGet]
    [Authorize(Roles = RoleNames.FirmAdminAndAbove)]
    public async Task<IActionResult> GetAll([FromQuery] string? searchText, [FromQuery] bool activeOnly = true)
    {
        var statuses = await mediator.Send(new GetAllCaseStatusesQuery(searchText, activeOnly));
        return Ok(ApiResponse<List<CaseStatusDTO>>.SuccessResponse(statuses));
    }

    // =====================================================
    // GET CASE STATUS BY ID — FirmAdmin and above ONLY
    // Fetches a single status's details by its ID.
    // =====================================================
    [HttpGet("{id}")]
    [Authorize(Roles = RoleNames.FirmAdminAndAbove)]
    public async Task<IActionResult> GetById(int id)
    {
        var status = await mediator.Send(new GetCaseStatusByIdQuery(id));
        return Ok(ApiResponse<CaseStatusDTO>.SuccessResponse(status));
    }

    // =====================================================
    // CREATE CASE STATUS — FirmAdmin and above
    // Adds a new workflow status. A FirmAdmin's new status is scoped to
    // their own firm; only a SuperAdmin can create a global status.
    // =====================================================
    [HttpPost]
    [Authorize(Roles = RoleNames.FirmAdminAndAbove)]
    public async Task<IActionResult> Create(CreateCaseStatusCommand command)
    {
        var id = await mediator.Send(command);
        return Ok(ApiResponse<int>.SuccessResponse(id, "Case status created successfully."));
    }

    // =====================================================
    // UPDATE CASE STATUS — FirmAdmin and above
    // Edits an existing status's name/sequence/colour/closed flag. Route
    // id and body StatusID must match.
    // =====================================================
    [HttpPut("{id}")]
    [Authorize(Roles = RoleNames.FirmAdminAndAbove)]
    public async Task<IActionResult> Update(int id, UpdateCaseStatusCommand command)
    {
        if (id != command.StatusID)
            return BadRequest(ApiResponse<bool>.FailureResponse("Route ID and body StatusID do not match."));

        var result = await mediator.Send(command);
        return Ok(ApiResponse<bool>.SuccessResponse(result, "Case status updated successfully."));
    }

    // =====================================================
    // DELETE CASE STATUS — FirmAdmin and above
    // Removes a status the firm no longer uses. Ownership/in-use checks
    // are enforced in the handler.
    // =====================================================
    [HttpDelete("{id}")]
    [Authorize(Roles = RoleNames.FirmAdminAndAbove)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await mediator.Send(new DeleteCaseStatusCommand(id));
        return Ok(ApiResponse<bool>.SuccessResponse(result, "Case status deleted successfully."));
    }
}
