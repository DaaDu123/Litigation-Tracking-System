using LTSBackend.Comman.Responses;
using LTSBackend.Features.Authorization;
using LTSBackend.Features.Cases.Commands.CreateCase;
using LTSBackend.Features.Cases.Commands.DeleteCase;
using LTSBackend.Features.Cases.Commands.UpdateCase;
using LTSBackend.Features.Cases.Commands.UpdateCaseStatus;
using LTSBackend.Features.Cases.DTOs;
using LTSBackend.Features.Cases.Queries.GetAllCases;
using LTSBackend.Features.Cases.Queries.GetCaseById;
using LTSBackend.Features.Cases.Queries.GetCaseStatusHistory;
using LTSBackend.Models.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LTSBackend.Features.Cases.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class CasesController(IMediator _mediator, ILogger<CasesController> _logger) : ControllerBase
{
    // =====================================================
    // GET ALL CASES — role-based visibility
    // Returns a paged, filterable (search text, court, status, priority)
    // list of cases. Visibility is enforced in GetAllCasesHandler, not
    // just here:
    //   - SuperAdmin: all cases, every firm
    //   - FirmAdmin: every case within their own firm
    //   - Partner: every case within their own firm ("View Firm Case Directory")
    //   - AssociateLawyer / Moharrir / InternParalegal: only cases they are
    //     actively assigned to (CaseAssignments), scoped inside the handler
    // =====================================================
    [HttpGet]
    [Authorize(Roles = RoleNames.AllFirmUsersAndSuperAdmin)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? searchText,
        [FromQuery] int? courtID,
        [FromQuery] int? statusID,
        [FromQuery] string? priority,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        _logger.LogInformation("Get all cases request - Page: {PageNumber}", pageNumber);

        var query = new GetAllCasesQuery(searchText, courtID, statusID, priority, pageNumber, pageSize);
        var result = await _mediator.Send(query);

        return Ok(ApiResponse<PagedResult<CaseDTO>>.SuccessResponse(
            result,
            "Cases successfully fetched"));
    }

    // =====================================================
    // GET CASE BY ID — role-based access
    // Fetches one case's full details. Enforced in GetCaseByIdHandler, not
    // just here:
    //   - SuperAdmin: any case, any firm
    //   - FirmAdmin / Partner: any case within their own firm
    //   - AssociateLawyer / Moharrir / InternParalegal: only if actively
    //     assigned to this specific case — otherwise 404 (not 403, so the
    //     case's existence isn't disclosed to a user who shouldn't see it)
    // =====================================================
    [HttpGet("{id}")]
    [Authorize(Roles = RoleNames.AllFirmUsersAndSuperAdmin)]
    public async Task<IActionResult> GetById(long id)
    {
        _logger.LogInformation("Get case by ID: {CaseID}", id);

        var query = new GetCaseByIdQuery(id);
        var result = await _mediator.Send(query);

        if (result == null)
        {
            return NotFound(ApiResponse<CaseDTO>.FailureResponse("Case not found"));
        }

        return Ok(ApiResponse<CaseDTO>.SuccessResponse(result, "Case successfully fetched"));
    }

    // =====================================================
    // CREATE NEW CASE — SuperAdmin, FirmAdmin, Partner only
    // Registers a brand-new litigation case with its mandatory details
    // (parties/court/category are resolved-or-created inside the handler
    // from either a picked ID or a typed name). Case-number uniqueness and
    // date-sanity rules (e.g. Expected Disposal Date can't be in the past)
    // are enforced by CreateCaseValidator before the handler ever runs.
    // =====================================================
    [HttpPost]
    [Authorize(Roles = RoleNames.PartnerAndAbove)]
    public async Task<IActionResult> Create([FromBody] CreateCaseDTO dto)
    {
        _logger.LogInformation("Create case: {CaseNumber}", dto.CaseNumber);

        var command = new CreateCaseCommand(
            dto.CaseNumber,
            dto.CaseTitle,
            dto.CaseDescription,
            dto.CourtID,
            dto.CourtName,
            dto.CategoryID,
            dto.CategoryName,
            dto.Priority,
            dto.SubjectMatter,
            dto.FilingDate,
            dto.InstitutionDate,
            dto.RegistrationDate,
            dto.ExpectedDisposalDate,
            dto.ClaimedAmount,
            dto.PotentialLiability,
            dto.FinancialImplication,
            dto.ResponsibleDepartmentID,
            dto.DepartmentName,
            dto.CurrentLegalOfficerID);

        var caseID = await _mediator.Send(command);

        return CreatedAtAction(
            nameof(GetById),
            new { id = caseID },
            ApiResponse<long>.SuccessResponse(caseID, "Case successfully created"));
    }

    // =====================================================
    // UPDATE CASE — SuperAdmin, FirmAdmin, Partner only
    // Edits an existing case's core details (title, description, court,
    // category, stage, priority, disposal date, financials, current legal
    // officer, archived flag). Route id and body CaseID must match.
    // =====================================================
    [HttpPut("{id}")]
    [Authorize(Roles = RoleNames.PartnerAndAbove)]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateCaseDTO dto)
    {
        _logger.LogInformation("Update case: {CaseID}", id);

        if (id != dto.CaseID)
        {
            return BadRequest(ApiResponse<bool>.FailureResponse(
                "URL and body case ID do not match"));
        }

        var command = new UpdateCaseCommand(
            dto.CaseID,
            dto.CaseNumber,
            dto.CaseTitle,
            dto.CaseDescription,
            dto.CourtID,
            dto.CategoryID,
            dto.StageID,
            dto.Priority,
            dto.SubjectMatter,
            dto.ExpectedDisposalDate,
            dto.ClaimedAmount,
            dto.PotentialLiability,
            dto.CurrentLegalOfficerID,
            dto.IsArchived);

        var result = await _mediator.Send(command);

        return Ok(ApiResponse<bool>.SuccessResponse(result, "Case successfully updated"));
    }

    // =====================================================
    // DELETE CASE — FirmAdmin and Partner
    // Deletes a case record outright.
    //
    // RESOLVED (previously flagged as out-of-sync): AppDbContext.
    // SeedRolePermissions grants Partner the DeleteCases permission, and
    // per current policy Partner has FirmAdmin-equivalent access across
    // the firm, so RoleNames.FirmAdminAndAbove (which now includes
    // Partner) is the correct, in-sync gate here.
    // =====================================================
    [HttpDelete("{id}")]
    [Authorize(Roles = RoleNames.FirmAdminAndAbove)]
    public async Task<IActionResult> Delete(long id)
    {
        _logger.LogInformation("Delete case: {CaseID}", id);

        var command = new DeleteCaseCommand(id);
        var result = await _mediator.Send(command);

        return Ok(ApiResponse<bool>.SuccessResponse(result, "Case successfully deleted"));
    }

    // =====================================================
    // UPDATE CASE STATUS — SuperAdmin, FirmAdmin, Partner only
    // Changes a case's current status (e.g. Active → Closed) and records
    // the transition. A row is written to CaseStatusHistory (old status,
    // new status, who changed it, remarks) so the read endpoint below has
    // a full timeline to show.
    // =====================================================
    [HttpPut("{id}/status")]
    [Authorize(Roles = RoleNames.PartnerAndAbove)]
    public async Task<IActionResult> UpdateStatus(
        long id,
        [FromBody] UpdateCaseStatusRequest request)
    {
        _logger.LogInformation("Update case status: {CaseID}", id);

        var command = new UpdateCaseStatusCommand(id, request.NewStatusID, request.Remarks);
        var result = await _mediator.Send(command);

        return Ok(ApiResponse<bool>.SuccessResponse(result, "Case status successfully updated"));
    }

    // =====================================================
    // GET CASE STATUS HISTORY — same visibility as GetById
    // Read side of FR-05 ("System shall maintain case status history").
    // Every status change is already recorded by CreateCaseHandler /
    // UpdateCaseStatusHandler; this is the endpoint that lets the UI read
    // that timeline back (old status, new status, who changed it, when,
    // remarks). Visibility is enforced in the handler, same rule as
    // GetById above.
    // =====================================================
    [HttpGet("{id}/status-history")]
    [Authorize(Roles = RoleNames.AllFirmUsersAndSuperAdmin)]
    public async Task<IActionResult> GetStatusHistory(long id)
    {
        _logger.LogInformation("Get case status history: {CaseID}", id);

        var query = new GetCaseStatusHistoryQuery(id);
        var result = await _mediator.Send(query);

        return Ok(ApiResponse<List<CaseStatusHistoryDTO>>.SuccessResponse(result, "Case status history successfully fetched"));
    }
}
