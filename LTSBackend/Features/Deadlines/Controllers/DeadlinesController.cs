using LTSBackend.Comman.Responses;
using LTSBackend.Features.Deadlines.Commands.CompleteDeadline;
using LTSBackend.Features.Deadlines.Commands.CreateDeadline;
using LTSBackend.Features.Deadlines.Commands.DeleteDeadline;
using LTSBackend.Features.Deadlines.Commands.UpdateDeadline;
using LTSBackend.Features.Deadlines.DTOs;
using LTSBackend.Features.Deadlines.Queries.GetCaseDeadlines;
using LTSBackend.Features.Deadlines.Queries.GetUpcomingDeadlines;
using LTSBackend.Models.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LTSBackend.Features.Deadlines.Controllers;

/// <summary>
/// SRS Reference: Complete Database Schema - Deadlines table
/// Litigation_Tracking_System_Case_SRS.docx Section 5.3 "Hearing and Deadline Monitoring"
/// FR-07, FR-08 - Track legal deadlines and generate reminders/alerts
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class DeadlinesController(IMediator _mediator) : ControllerBase
{
    // =====================================================
    // GET UPCOMING DEADLINES — Any firm user
    // Returns deadlines due within the next N days (daysAhead, optional)
    // across the caller's visible cases, for the deadline-tracker widget.
    // =====================================================
    [HttpGet("upcoming")]
    [Authorize(Roles = RoleNames.AllFirmUsers)]
    public async Task<IActionResult> GetUpcoming([FromQuery] int? daysAhead)
    {
        var result = await _mediator.Send(new GetUpcomingDeadlinesQuery { DaysAhead = daysAhead });
        return Ok(ApiResponse<List<DeadlineDetailDTO>>.SuccessResponse(result, "Upcoming deadlines fetched"));
    }

    // =====================================================
    // GET DEADLINES FOR A CASE — Any firm user
    // Lists a specific case's deadlines. Pass completed=true/false to
    // filter to only completed or only pending ones.
    // =====================================================
    [HttpGet("case/{caseId}")]
    [Authorize(Roles = RoleNames.AllFirmUsers)]
    public async Task<IActionResult> GetByCase(long caseId, [FromQuery] bool? completed)
    {
        var result = await _mediator.Send(new GetCaseDeadlinesQuery { CaseID = caseId, Completed = completed });
        return Ok(ApiResponse<List<DeadlineDetailDTO>>.SuccessResponse(result, "Case deadlines fetched"));
    }

    // =====================================================
    // CREATE DEADLINE — Lawyer roles
    // Adds a new deadline against a case, with a due date and the user's
    // own choice of ReminderDays (how many days before the due date they
    // want to be warned — see CreateDeadlineValidator/ReminderService for
    // how that value is used and its one-week guaranteed floor).
    // =====================================================
    [HttpPost]
    [Authorize(Roles = RoleNames.AllLawyers)]
    public async Task<IActionResult> Create([FromBody] CreateDeadlineDTO dto)
    {
        var id = await _mediator.Send(new CreateDeadlineCommand { Deadline = dto });
        return CreatedAtAction(nameof(GetByCase), new { caseId = dto.CaseID }, ApiResponse<long>.SuccessResponse(id, "Deadline successfully created"));
    }

    // =====================================================
    // UPDATE DEADLINE — Lawyer roles
    // Edits an existing deadline's type, due date, reminder window, or
    // remarks.
    // =====================================================
    [HttpPut("{id}")]
    [Authorize(Roles = RoleNames.AllLawyers)]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateDeadlineDTO dto)
    {
        dto.DeadlineID = id;
        var result = await _mediator.Send(new UpdateDeadlineCommand { Deadline = dto });
        return Ok(ApiResponse<bool>.SuccessResponse(result, "Deadline successfully updated"));
    }

    // =====================================================
    // COMPLETE DEADLINE — Lawyer roles
    // Marks a deadline as completed so it stops appearing in "upcoming"
    // lists and stops generating reminder emails.
    // =====================================================
    [HttpPut("{id}/complete")]
    [Authorize(Roles = RoleNames.AllLawyers)]
    public async Task<IActionResult> Complete(long id)
    {
        var result = await _mediator.Send(new CompleteDeadlineCommand { DeadlineID = id });
        return Ok(ApiResponse<bool>.SuccessResponse(result, "Deadline marked complete"));
    }

    // =====================================================
    // DELETE DEADLINE — Partner and above
    // Permanently removes a deadline. Restricted to Partner-and-above
    // since deleting a legal deadline is a higher-risk action than
    // creating/updating/completing one.
    // =====================================================
    [HttpDelete("{id}")]
    [Authorize(Roles = RoleNames.PartnerAndAbove)]
    public async Task<IActionResult> Delete(long id)
    {
        var result = await _mediator.Send(new DeleteDeadlineCommand { DeadlineID = id });
        return Ok(ApiResponse<bool>.SuccessResponse(result, "Deadline successfully deleted"));
    }
}
