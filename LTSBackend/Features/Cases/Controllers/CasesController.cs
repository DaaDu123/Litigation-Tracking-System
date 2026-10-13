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


        if (archivedOnly && !(User.IsInRole(RoleNames.FirmAdmin) || User.IsInRole(RoleNames.Partner)))
            return Forbid();

        var query = new GetAllCasesQuery(searchText, courtID, statusID, priority, pageNumber, pageSize, archivedOnly);
        var result = await _mediator.Send(query);

        return Ok(ApiResponse<PagedResult<CaseDTO>>.SuccessResponse(
            result,
            "Cases successfully fetched"));
    }











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








    [HttpDelete("{id}")]
    [Authorize(Roles = RoleNames.FirmAdminAndAbove)]
    public async Task<IActionResult> Archive(long id, [FromQuery] string? reason = null)
    {
        _logger.LogInformation("Archive case: {CaseID}", id);

        var result = await _mediator.Send(new ArchiveCaseCommand(id, reason));

        return Ok(ApiResponse<bool>.SuccessResponse(result, "Case archived"));
    }




    [HttpPost("{id}/restore")]
    [Authorize(Roles = RoleNames.FirmAdminAndAbove)]
    public async Task<IActionResult> Restore(long id)
    {
        _logger.LogInformation("Restore case: {CaseID}", id);

        var result = await _mediator.Send(new RestoreCaseCommand(id));

        return Ok(ApiResponse<bool>.SuccessResponse(result, "Case restored"));
    }







    [HttpPost("{id}/permanent-delete")]
    [Authorize(Roles = RoleNames.FirmAdminOnly)]
    public async Task<IActionResult> PermanentDelete(long id, [FromBody] PermanentDeleteCaseDTO dto)
    {
        _logger.LogWarning("Permanent delete requested: {CaseID}", id);

        var result = await _mediator.Send(new DeleteCaseCommand(id, dto.ConfirmCaseNumber, dto.Reason));

        return Ok(ApiResponse<bool>.SuccessResponse(result, "Case permanently deleted"));
    }








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










    [HttpGet("{id}/status-history")]
    [Authorize(Roles = RoleNames.AllFirmUsersAndSuperAdmin)]
    public async Task<IActionResult> GetStatusHistory(long id)
    {
        _logger.LogInformation("Get case status history: {CaseID}", id);

        var query = new GetCaseStatusHistoryQuery(id);
        var result = await _mediator.Send(query);

        return Ok(ApiResponse<List<CaseStatusHistoryDTO>>.SuccessResponse(result, "Case status history successfully fetched"));
    }







    [HttpPost("{id}/workflow/generate")]
    [Authorize(Roles = RoleNames.FirmAdminAndAbove)]
    public async Task<IActionResult> GenerateWorkflow(long id)
    {
        _logger.LogInformation("Generate workflow for case: {CaseID}", id);

        var result = await _mediator.Send(new GenerateCaseWorkflowCommand(id));

        return Ok(ApiResponse<bool>.SuccessResponse(result, "Workflow generated"));
    }






    [HttpGet("{id}/workflow")]
    [Authorize(Roles = RoleNames.AllFirmUsersAndSuperAdmin)]
    public async Task<IActionResult> GetWorkflow(long id)
    {
        var result = await _mediator.Send(new GetCaseWorkflowQuery(id));
        return Ok(ApiResponse<CaseWorkflowDTO>.SuccessResponse(result, "Case workflow successfully fetched"));
    }
}
