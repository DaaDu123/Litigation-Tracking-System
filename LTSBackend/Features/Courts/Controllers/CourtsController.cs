using LTSBackend.Comman.Responses;
using LTSBackend.Features.Courts.Commands.CreateCourt;
using LTSBackend.Features.Courts.Commands.DeleteCourt;
using LTSBackend.Features.Courts.Commands.UpdateCourt;
using LTSBackend.Features.Courts.DTOs;
using LTSBackend.Features.Courts.Queries.GetAllCourts;
using LTSBackend.Features.Courts.Queries.GetCourtById;
using LTSBackend.Models.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LTSBackend.Features.Courts.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class CourtsController(IMediator mediator) : ControllerBase
{
    // =====================================================
    // GET ALL COURTS — FirmAdmin and above ONLY
    // Master-data management is FirmAdmin and Partner's task.
    // AssociateLawyer, Moharrir, InternParalegal and SuperAdmin have NO
    // access (not even read) to this master data admin surface. Query
    // results are automatically scoped (system-wide global courts + the
    // caller's own firm's custom courts) via the HasQueryFilter on Court
    // in AppDbContext — not by anything in this controller.
    // =====================================================
    [HttpGet]
    [Authorize(Roles = RoleNames.FirmAdminAndAbove)]
    public async Task<IActionResult> GetAll([FromQuery] string? searchText,[FromQuery] bool activeOnly = true)
    {
        var courts = await mediator.Send(new GetAllCourtsQuery(searchText, activeOnly));
        return Ok(ApiResponse<List<CourtDTO>>.SuccessResponse(courts));
    }

    // =====================================================
    // GET COURT BY ID — FirmAdmin and above ONLY
    // Fetches a single court's details by its ID.
    // =====================================================
    [HttpGet("{id}")]
    [Authorize(Roles = RoleNames.FirmAdminAndAbove)]
    public async Task<IActionResult> GetById(int id)
    {
        var court = await mediator.Send(new GetCourtByIdQuery(id));
        return Ok(ApiResponse<CourtDTO>.SuccessResponse(court));
    }

    // =====================================================
    // CREATE COURT — FirmAdmin and above
    // Adds a new court. Court is per-tenant-aware: FirmID is nullable on
    // the Court entity — NULL means a system-wide global court (managed by
    // SuperAdmin, visible to every firm), a real value means a firm's own
    // custom court entry. CreateCourtHandler sets FirmID = null for
    // SuperAdmin or the caller's own FirmID for FirmAdmin. Requires the EF
    // migration that adds the Court.FirmID column to be applied.
    // =====================================================
    [HttpPost]
    [Authorize(Roles = RoleNames.FirmAdminAndAbove)]
    public async Task<IActionResult> Create(CreateCourtCommand command)
    {
        var id = await mediator.Send(command);
        return Ok(ApiResponse<int>.SuccessResponse(id, "Court created successfully."));
    }

    // =====================================================
    // UPDATE COURT — FirmAdmin and above
    // Edits an existing court's details. A FirmAdmin may only update
    // their OWN firm's custom court (enforced in UpdateCourtHandler) —
    // never a global or another firm's court.
    // =====================================================
    [HttpPut("{id}")]
    [Authorize(Roles = RoleNames.FirmAdminAndAbove)]
    public async Task<IActionResult> Update(int id, UpdateCourtCommand command)
    {
        if (id != command.CourtID)
            return BadRequest(ApiResponse<bool>.FailureResponse("Route ID and body CourtID do not match."));

        var result = await mediator.Send(command);
        return Ok(ApiResponse<bool>.SuccessResponse(result, "Court updated successfully."));
    }

    // =====================================================
    // DELETE COURT — FirmAdmin and above
    // Removes a court the firm no longer needs. A FirmAdmin may only
    // delete their OWN firm's custom court (enforced in
    // DeleteCourtHandler) — never a global or another firm's court.
    // =====================================================
    [HttpDelete("{id}")]
    [Authorize(Roles = RoleNames.FirmAdminAndAbove)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await mediator.Send(new DeleteCourtCommand(id));
        return Ok(ApiResponse<bool>.SuccessResponse(result, "Court deleted successfully."));
    }
}
