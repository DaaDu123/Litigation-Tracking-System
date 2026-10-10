using LTSBackend.Comman.Responses;
using LTSBackend.Features.Authorization;
using LTSBackend.Features.Cases.Commands.ArchiveCase;
using LTSBackend.Features.Cases.Commands.CreateCase;
using LTSBackend.Features.Cases.Commands.DeleteCase;
using LTSBackend.Features.Cases.Commands.GenerateCaseWorkflow;
using LTSBackend.Features.Cases.Commands.RestoreCase;
using LTSBackend.Features.Cases.Commands.UpdateCase;
using LTSBackend.Features.Cases.Commands.UpdateCaseStatus;
using LTSBackend.Features.Cases.DTOs;
using LTSBackend.Features.Cases.Queries.GetAllCases;
using LTSBackend.Features.Cases.Queries.GetCaseById;
using LTSBackend.Features.Cases.Queries.GetCaseWorkflow;
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
        [FromQuery] int pageSize = 10,
        [FromQuery] bool archivedOnly = false)
    {
        _logger.LogInformation("Get all cases request - Page: {PageNumber}", pageNumber);

        // The archive (restore) view is for senior firm management only.
        if (archivedOnly && !(User.IsInRole(RoleNames.FirmAdmin) || User.IsInRole(RoleNames.Partner)))
            return Forbid();

        var query = new GetAllCasesQuery(searchText, courtID, statusID, priority, pageNumber, pageSize, archivedOnly);
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
    // Registers a brand-new litigation case with its mandatory details.
    // Court/Category are picked from existing master data; the category's
    // Case Workflow Template then generates the status, stages, default
    // department and document checklist in one transaction. Case-number uniqueness and
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
            dto.CategoryID,
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
            dto.CurrentLegalOfficerID);

        var result = await _mediator.Send(command);

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.CaseID },
            ApiResponse<CreateCaseResultDTO>.SuccessResponse(result, "Case successfully created"));
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
            dto.IsArchived,
            dto.ResponsibleDepartmentID);

        var result = await _mediator.Send(command);

        return Ok(ApiResponse<bool>.SuccessResponse(result, "Case successfully updated"));
    }

    // =====================================================
    // DELETE CASE (normal) = ARCHIVE / SOFT DELETE — FirmAdmin and Partner
    // Nothing is removed: the case disappears from normal lists but all
    // data and files stay, and it can be restored. Optional reason is
    // written to the audit log. Permanent deletion is a separate,
    // FirmAdmin-only endpoint below.
    // =====================================================
    [HttpDelete("{id}")]
    [Authorize(Roles = RoleNames.FirmAdminAndAbove)]
    public async Task<IActionResult> Archive(long id, [FromQuery] string? reason = null)
    {
        _logger.LogInformation("Archive case: {CaseID}", id);

        var result = await _mediator.Send(new ArchiveCaseCommand(id, reason));

        return Ok(ApiResponse<bool>.SuccessResponse(result, "Case archived"));
    }

    // =====================================================
    // RESTORE CASE — FirmAdmin and Partner
    // =====================================================
    [HttpPost("{id}/restore")]
    [Authorize(Roles = RoleNames.FirmAdminAndAbove)]
    public async Task<IActionResult> Restore(long id)
    {
        _logger.LogInformation("Restore case: {CaseID}", id);

        var result = await _mediator.Send(new RestoreCaseCommand(id));

        return Ok(ApiResponse<bool>.SuccessResponse(result, "Case restored"));
    }

    // =====================================================
    // PERMANENT DELETE — FirmAdmin ONLY (Partner excluded on purpose)
    // Irreversible. Only an already-archived case, with the exact case
    // number typed as confirmation and a written reason; audited. Posted
    // (not DELETE-with-body) so every HTTP client/proxy can send the body.
    // =====================================================
    [HttpPost("{id}/permanent-delete")]
    [Authorize(Roles = RoleNames.FirmAdminOnly)]
    public async Task<IActionResult> PermanentDelete(long id, [FromBody] PermanentDeleteCaseDTO dto)
    {
        _logger.LogWarning("Permanent delete requested: {CaseID}", id);

        var result = await _mediator.Send(new DeleteCaseCommand(id, dto.ConfirmCaseNumber, dto.Reason));

        return Ok(ApiResponse<bool>.SuccessResponse(result, "Case permanently deleted"));
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

    // =====================================================
    // GENERATE WORKFLOW (backfill) — FirmAdmin and Partner
    // Gives a legacy case (created before workflow templates) its
    // stages + document checklist from the category template. Does not
    // change status/stage/department/documents.
    // =====================================================
    [HttpPost("{id}/workflow/generate")]
    [Authorize(Roles = RoleNames.FirmAdminAndAbove)]
    public async Task<IActionResult> GenerateWorkflow(long id)
    {
        _logger.LogInformation("Generate workflow for case: {CaseID}", id);

        var result = await _mediator.Send(new GenerateCaseWorkflowCommand(id));

        return Ok(ApiResponse<bool>.SuccessResponse(result, "Workflow generated"));
    }

    // =====================================================
    // GET CASE WORKFLOW — same visibility as GetById
    // The case's own stage progress (Pending/Active/Completed) plus its
    // required/optional document checklist with fulfilment state.
    // =====================================================
    [HttpGet("{id}/workflow")]
    [Authorize(Roles = RoleNames.AllFirmUsersAndSuperAdmin)]
    public async Task<IActionResult> GetWorkflow(long id)
    {
        var result = await _mediator.Send(new GetCaseWorkflowQuery(id));
        return Ok(ApiResponse<CaseWorkflowDTO>.SuccessResponse(result, "Case workflow successfully fetched"));
    }
}
