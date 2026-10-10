using LTSBackend.Comman.Responses;
using LTSBackend.Features.CaseWorkflowTemplates.Commands.DeleteCaseWorkflowTemplate;
using LTSBackend.Features.CaseWorkflowTemplates.Commands.SaveCaseWorkflowTemplate;
using LTSBackend.Features.CaseWorkflowTemplates.DTOs;
using LTSBackend.Features.CaseWorkflowTemplates.Queries.GetAllCaseWorkflowTemplates;
using LTSBackend.Features.CaseWorkflowTemplates.Queries.GetCaseWorkflowTemplateByCategory;
using LTSBackend.Models.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LTSBackend.Features.CaseWorkflowTemplates.Controllers;

/// <summary>
/// Master-data configuration of the Case Workflow Template: per Case
/// Category, the default Department, initial Status, ordered Stages and
/// required/optional Document Types a new Case is generated from. Same
/// per-tenant model as the other masters (global + own firm).
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class CaseWorkflowTemplatesController(IMediator mediator) : ControllerBase
{
    // LIST — FirmAdmin / Partner (master-data administration)
    [HttpGet]
    [Authorize(Roles = RoleNames.FirmAdminAndAbove)]
    public async Task<IActionResult> GetAll([FromQuery] bool activeOnly = false)
    {
        var result = await mediator.Send(new GetAllCaseWorkflowTemplatesQuery(activeOnly));
        return Ok(ApiResponse<List<CaseWorkflowTemplateDTO>>.SuccessResponse(result));
    }

    // RESOLVE FOR A CATEGORY — everyone who can create a case (New Case wizard preview).
    // Data is null when the category has no template yet (the case is then created with the default New status / Filing stage and no checklist).
    [HttpGet("by-category/{categoryId}")]
    [Authorize(Roles = RoleNames.PartnerAndAbove)]
    public async Task<IActionResult> GetByCategory(int categoryId)
    {
        var result = await mediator.Send(new GetCaseWorkflowTemplateByCategoryQuery(categoryId));
        return Ok(ApiResponse<CaseWorkflowTemplateDTO?>.SuccessResponse(result!,
            result == null ? "No workflow template is configured for this category." : "Workflow template fetched."));
    }

    // SAVE (create or replace the caller's template for the category)
    [HttpPost]
    [Authorize(Roles = RoleNames.FirmAdminAndAbove)]
    public async Task<IActionResult> Save([FromBody] SaveCaseWorkflowTemplateDTO dto)
    {
        var id = await mediator.Send(new SaveCaseWorkflowTemplateCommand(
            dto.CategoryID, dto.DefaultDepartmentID, dto.InitialStatusID,
            dto.StageIDs, dto.RequiredDocumentTypeIDs, dto.OptionalDocumentTypeIDs, dto.IsActive));
        return Ok(ApiResponse<int>.SuccessResponse(id, "Workflow template saved successfully."));
    }

    // DELETE (own scope only)
    [HttpDelete("{id}")]
    [Authorize(Roles = RoleNames.FirmAdminAndAbove)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await mediator.Send(new DeleteCaseWorkflowTemplateCommand(id));
        return Ok(ApiResponse<bool>.SuccessResponse(result, "Workflow template deleted successfully."));
    }
}
