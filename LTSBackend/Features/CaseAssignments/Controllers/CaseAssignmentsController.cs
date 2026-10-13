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





[Route("api/[controller]")]
[ApiController]
[Authorize]
public class CaseAssignmentsController(IMediator _mediator) : ControllerBase
{






    [HttpGet("case/{caseId}")]
    [Authorize(Roles = RoleNames.AllFirmUsers)]
    public async Task<IActionResult> GetByCase(long caseId, [FromQuery] bool activeOnly = false)
    {
        var result = await _mediator.Send(new GetCaseAssignmentsQuery { CaseID = caseId, ActiveOnly = activeOnly });
        return Ok(ApiResponse<List<CaseAssignmentDetailDTO>>.SuccessResponse(result, "Case assignments fetched"));
    }








    [HttpGet("my-cases")]
    [Authorize(Roles = RoleNames.AllFirmUsers)]
    public async Task<IActionResult> GetMyAssignedCases()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
        {


            return Unauthorized(ApiResponse<object>.FailureResponse("Unable to determine current user."));
        }

        var result = await _mediator.Send(new GetMyAssignedCasesQuery { UserID = userId });
        return Ok(ApiResponse<List<CaseAssignmentDetailDTO>>.SuccessResponse(result, "Assigned cases fetched"));
    }







    [HttpPost]
    [Authorize(Roles = RoleNames.CanAssignCases)]
    public async Task<IActionResult> Assign([FromBody] AssignCaseDTO dto)
    {
        var id = await _mediator.Send(new AssignCaseCommand { Assignment = dto });
        return CreatedAtAction(nameof(GetByCase), new { caseId = dto.CaseID }, ApiResponse<long>.SuccessResponse(id, "Case successfully assigned"));
    }






    [HttpPut("{id}")]
    [Authorize(Roles = RoleNames.CanAssignCases)]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateAssignmentDTO dto)
    {
        dto.AssignmentID = id;
        var result = await _mediator.Send(new UpdateAssignmentCommand { Assignment = dto });
        return Ok(ApiResponse<bool>.SuccessResponse(result, "Assignment successfully updated"));
    }







    [HttpPut("{id}/end")]
    [Authorize(Roles = RoleNames.CanAssignCases)]
    public async Task<IActionResult> End(long id, [FromBody] string? remarks)
    {
        var result = await _mediator.Send(new EndAssignmentCommand { AssignmentID = id, Remarks = remarks });
        return Ok(ApiResponse<bool>.SuccessResponse(result, "Assignment successfully ended"));
    }
}
