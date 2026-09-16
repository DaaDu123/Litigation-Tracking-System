using System.Security.Claims;
using LTSBackend.Comman.Responses;
using LTSBackend.Features.CaseAssignments.Commands.AssignCase;
using LTSBackend.Features.CaseAssignments.Commands.EndAssignment;
using LTSBackend.Features.CaseAssignments.Commands.UpdateAssignment;
using LTSBackend.Features.CaseAssignments.DTOs;
using LTSBackend.Features.CaseAssignments.Queries.GetCaseAssignments;
using LTSBackend.Features.CaseAssignments.Queries.GetMyAssignedCases;
using LTSBackend.Models.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LTSBackend.Features.CaseAssignments.Controllers;

/// <summary>
/// SRS Reference: Litigation_Tracking_System_Case_SRS.docx UC-04 "Assign Case to Counsel"
/// Section 5.10.2 Lawyer Dashboard - "My Cases Panel"
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class CaseAssignmentsController(IMediator _mediator) : ControllerBase
{
    // =====================================================
    // GET ASSIGNMENTS FOR A CASE — Any firm user
    // Lists everyone (lawyer, supervisor, external counsel, etc.) who has
    // ever been assigned to the given case. Pass activeOnly=true to only
    // see currently-active assignments (ended ones filtered out).
    // =====================================================
    [HttpGet("case/{caseId}")]
    [Authorize(Roles = RoleNames.AllFirmUsers)]
    public async Task<IActionResult> GetByCase(long caseId, [FromQuery] bool activeOnly = false)
    {
        var result = await _mediator.Send(new GetCaseAssignmentsQuery { CaseID = caseId, ActiveOnly = activeOnly });
        return Ok(ApiResponse<List<CaseAssignmentDetailDTO>>.SuccessResponse(result, "Case assignments fetched"));
    }

    // =====================================================
    // GET MY ASSIGNED CASES — Any firm user
    // Returns the currently logged-in user's own active case assignments,
    // for their personal "My Cases" dashboard panel. The user ID is taken
    // from their own JWT claim, so this only ever returns the caller's own
    // cases, never another user's.
    // =====================================================
    [HttpGet("my-cases")]
    [Authorize(Roles = RoleNames.AllFirmUsers)]
    public async Task<IActionResult> GetMyAssignedCases()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
        {
            // Fail closed rather than silently querying with UserID = 0.
            // [Authorize] should make this unreachable in practice.
            return Unauthorized(ApiResponse<object>.FailureResponse("Unable to determine current user."));
        }

        var result = await _mediator.Send(new GetMyAssignedCasesQuery { UserID = userId });
        return Ok(ApiResponse<List<CaseAssignmentDetailDTO>>.SuccessResponse(result, "Assigned cases fetched"));
    }

    // =====================================================
    // ASSIGN CASE TO COUNSEL — Roles allowed to assign cases (SRS UC-04)
    // Creates a new case assignment, linking a lawyer/supervisor/external
    // counsel to a case (optionally as lead counsel). Multiple users can be
    // assigned to the same case at once.
    // =====================================================
    [HttpPost]
    [Authorize(Roles = RoleNames.CanAssignCases)]
    public async Task<IActionResult> Assign([FromBody] AssignCaseDTO dto)
    {
        var id = await _mediator.Send(new AssignCaseCommand { Assignment = dto });
        return CreatedAtAction(nameof(GetByCase), new { caseId = dto.CaseID }, ApiResponse<long>.SuccessResponse(id, "Case successfully assigned"));
    }

    // =====================================================
    // UPDATE ASSIGNMENT — Roles allowed to assign cases
    // Edits an existing assignment's details (e.g. assignment type, lead
    // counsel flag, remarks) without ending it.
    // =====================================================
    [HttpPut("{id}")]
    [Authorize(Roles = RoleNames.CanAssignCases)]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateAssignmentDTO dto)
    {
        dto.AssignmentID = id;
        var result = await _mediator.Send(new UpdateAssignmentCommand { Assignment = dto });
        return Ok(ApiResponse<bool>.SuccessResponse(result, "Assignment successfully updated"));
    }

    // =====================================================
    // END ASSIGNMENT — Roles allowed to assign cases
    // Closes out an assignment (sets its EndDate) without deleting the
    // historical record — used when reassigning a case to someone new, so
    // the old assignment's history is preserved.
    // =====================================================
    [HttpPut("{id}/end")]
    [Authorize(Roles = RoleNames.CanAssignCases)]
    public async Task<IActionResult> End(long id, [FromBody] string? remarks)
    {
        var result = await _mediator.Send(new EndAssignmentCommand { AssignmentID = id, Remarks = remarks });
        return Ok(ApiResponse<bool>.SuccessResponse(result, "Assignment successfully ended"));
    }
}
