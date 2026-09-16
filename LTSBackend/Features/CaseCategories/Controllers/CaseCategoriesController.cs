using LTSBackend.Comman.Responses;
using LTSBackend.Features.CaseCategories.Commands.CreateCaseCategory;
using LTSBackend.Features.CaseCategories.Commands.DeleteCaseCategory;
using LTSBackend.Features.CaseCategories.Commands.UpdateCaseCategory;
using LTSBackend.Features.CaseCategories.DTOs;
using LTSBackend.Features.CaseCategories.Queries.GetAllCaseCategories;
using LTSBackend.Features.CaseCategories.Queries.GetCaseCategoryById;
using LTSBackend.Models.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LTSBackend.Features.CaseCategories.Controllers;

/// <summary>
/// Master data for case categories (e.g. Civil, Criminal, Corporate).
/// Same per-tenant model as Courts/Departments - FirmID is nullable on
/// CaseCategory: NULL is a system-wide global category managed by
/// SuperAdmin and visible to every firm, a real value is a firm's own
/// custom category visible/editable only by that firm. See
/// CreateCaseCategoryHandler/UpdateCaseCategoryHandler/DeleteCaseCategoryHandler
/// for the ownership enforcement.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class CaseCategoriesController(IMediator mediator) : ControllerBase
{
    // =====================================================
    // GET ALL CASE CATEGORIES — FirmAdmin and above ONLY
    // Master-data management is FirmAdmin and Partner's task.
    // AssociateLawyer, Moharrir, InternParalegal and SuperAdmin have NO
    // access (not even read) to this master data admin surface. Scoping to
    // global + own firm happens automatically via
    // the EF Core HasQueryFilter on CaseCategory, not in this controller.
    // =====================================================
    [HttpGet]
    [Authorize(Roles = RoleNames.FirmAdminAndAbove)]
    public async Task<IActionResult> GetAll([FromQuery] string? searchText, [FromQuery] bool activeOnly = true)
    {
        var categories = await mediator.Send(new GetAllCaseCategoriesQuery(searchText, activeOnly));
        return Ok(ApiResponse<List<CaseCategoryDTO>>.SuccessResponse(categories));
    }

    // =====================================================
    // GET CASE CATEGORY BY ID — FirmAdmin and above ONLY
    // Fetches a single category's details by its ID.
    // =====================================================
    [HttpGet("{id}")]
    [Authorize(Roles = RoleNames.FirmAdminAndAbove)]
    public async Task<IActionResult> GetById(int id)
    {
        var category = await mediator.Send(new GetCaseCategoryByIdQuery(id));
        return Ok(ApiResponse<CaseCategoryDTO>.SuccessResponse(category));
    }

    // =====================================================
    // CREATE CASE CATEGORY — FirmAdmin and above
    // Adds a new category. A FirmAdmin's new category is scoped to their
    // own firm only; only a SuperAdmin can create a global (FirmID = NULL)
    // category visible to every firm (enforced in the handler).
    // =====================================================
    [HttpPost]
    [Authorize(Roles = RoleNames.FirmAdminAndAbove)]
    public async Task<IActionResult> Create(CreateCaseCategoryCommand command)
    {
        var id = await mediator.Send(command);
        return Ok(ApiResponse<int>.SuccessResponse(id, "Case category created successfully."));
    }

    // =====================================================
    // UPDATE CASE CATEGORY — FirmAdmin and above
    // Edits an existing category's name/description/active flag. Route id
    // and body CategoryID must match. A FirmAdmin cannot edit another
    // firm's or a global category — that ownership check happens in the
    // handler.
    // =====================================================
    [HttpPut("{id}")]
    [Authorize(Roles = RoleNames.FirmAdminAndAbove)]
    public async Task<IActionResult> Update(int id, UpdateCaseCategoryCommand command)
    {
        if (id != command.CategoryID)
            return BadRequest(ApiResponse<bool>.FailureResponse("Route ID and body CategoryID do not match."));

        var result = await mediator.Send(command);
        return Ok(ApiResponse<bool>.SuccessResponse(result, "Case category updated successfully."));
    }

    // =====================================================
    // DELETE CASE CATEGORY — FirmAdmin and above
    // Removes a category the firm no longer needs. Ownership/ in-use
    // checks (e.g. can't delete a global category, or one still assigned
    // to existing cases) are enforced in the handler.
    // =====================================================
    [HttpDelete("{id}")]
    [Authorize(Roles = RoleNames.FirmAdminAndAbove)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await mediator.Send(new DeleteCaseCategoryCommand(id));
        return Ok(ApiResponse<bool>.SuccessResponse(result, "Case category deleted successfully."));
    }
}
