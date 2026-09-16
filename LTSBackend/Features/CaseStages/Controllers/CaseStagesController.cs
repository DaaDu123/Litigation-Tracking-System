using LTSBackend.Comman.Responses;
using LTSBackend.Features.CaseStages.Commands.CreateCaseStage;
using LTSBackend.Features.CaseStages.Commands.DeleteCaseStage;
using LTSBackend.Features.CaseStages.Commands.UpdateCaseStage;
using LTSBackend.Features.CaseStages.DTOs;
using LTSBackend.Features.CaseStages.Queries.GetAllCaseStages;
using LTSBackend.Features.CaseStages.Queries.GetCaseStageById;
using LTSBackend.Models.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LTSBackend.Features.CaseStages.Controllers;

/// <summary>
/// Master data for case stages (litigation stages). Same per-tenant model
/// as Courts/Departments/CaseCategories - see CreateCaseStageHandler/
/// UpdateCaseStageHandler/DeleteCaseStageHandler for the ownership rules.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class CaseStagesController(IMediator mediator) : ControllerBase
{
    // =====================================================
    // GET ALL CASE STAGES — FirmAdmin and above ONLY
    // Master-data management is FirmAdmin and Partner's task.
    // AssociateLawyer, Moharrir, InternParalegal and SuperAdmin have NO
    // access (not even read) to this master data admin surface.
    // =====================================================
    [HttpGet]
    [Authorize(Roles = RoleNames.FirmAdminAndAbove)]
    public async Task<IActionResult> GetAll([FromQuery] string? searchText, [FromQuery] bool activeOnly = true)
    {
        var stages = await mediator.Send(new GetAllCaseStagesQuery(searchText, activeOnly));
        return Ok(ApiResponse<List<CaseStageDTO>>.SuccessResponse(stages));
    }

    // =====================================================
    // GET CASE STAGE BY ID — FirmAdmin and above ONLY
    // Fetches a single stage's details by its ID.
    // =====================================================
    [HttpGet("{id}")]
    [Authorize(Roles = RoleNames.FirmAdminAndAbove)]
    public async Task<IActionResult> GetById(int id)
    {
        var stage = await mediator.Send(new GetCaseStageByIdQuery(id));
        return Ok(ApiResponse<CaseStageDTO>.SuccessResponse(stage));
    }

    // =====================================================
    // CREATE CASE STAGE — FirmAdmin and above
    // Adds a new litigation stage. A FirmAdmin's new stage is scoped to
    // their own firm; only a SuperAdmin can create a global stage.
    // =====================================================
    [HttpPost]
    [Authorize(Roles = RoleNames.FirmAdminAndAbove)]
    public async Task<IActionResult> Create(CreateCaseStageCommand command)
    {
        var id = await mediator.Send(command);
        return Ok(ApiResponse<int>.SuccessResponse(id, "Case stage created successfully."));
    }

    // =====================================================
    // UPDATE CASE STAGE — FirmAdmin and above
    // Edits an existing stage's name/description/active flag. Route id
    // and body StageID must match.
    // =====================================================
    [HttpPut("{id}")]
    [Authorize(Roles = RoleNames.FirmAdminAndAbove)]
    public async Task<IActionResult> Update(int id, UpdateCaseStageCommand command)
    {
        if (id != command.StageID)
            return BadRequest(ApiResponse<bool>.FailureResponse("Route ID and body StageID do not match."));

        var result = await mediator.Send(command);
        return Ok(ApiResponse<bool>.SuccessResponse(result, "Case stage updated successfully."));
    }

    // =====================================================
    // DELETE CASE STAGE — FirmAdmin and above
    // Removes a stage the firm no longer uses. Ownership/in-use checks are
    // enforced in the handler.
    // =====================================================
    [HttpDelete("{id}")]
    [Authorize(Roles = RoleNames.FirmAdminAndAbove)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await mediator.Send(new DeleteCaseStageCommand(id));
        return Ok(ApiResponse<bool>.SuccessResponse(result, "Case stage deleted successfully."));
    }
}
