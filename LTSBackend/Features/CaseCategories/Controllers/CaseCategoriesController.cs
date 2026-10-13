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










[Route("api/[controller]")]
[ApiController]
[Authorize]
public class CaseCategoriesController(IMediator mediator) : ControllerBase
{








    [HttpGet]
    [Authorize(Roles = RoleNames.FirmAdminAndAbove)]
    public async Task<IActionResult> GetAll([FromQuery] string? searchText, [FromQuery] bool activeOnly = true)
    {
        var categories = await mediator.Send(new GetAllCaseCategoriesQuery(searchText, activeOnly));
        return Ok(ApiResponse<List<CaseCategoryDTO>>.SuccessResponse(categories));
    }





    [HttpGet("{id}")]
    [Authorize(Roles = RoleNames.FirmAdminAndAbove)]
    public async Task<IActionResult> GetById(int id)
    {
        var category = await mediator.Send(new GetCaseCategoryByIdQuery(id));
        return Ok(ApiResponse<CaseCategoryDTO>.SuccessResponse(category));
    }







    [HttpPost]
    [Authorize(Roles = RoleNames.FirmAdminAndAbove)]
    public async Task<IActionResult> Create(CreateCaseCategoryCommand command)
    {
        var id = await mediator.Send(command);
        return Ok(ApiResponse<int>.SuccessResponse(id, "Case category created successfully."));
    }








    [HttpPut("{id}")]
    [Authorize(Roles = RoleNames.FirmAdminAndAbove)]
    public async Task<IActionResult> Update(int id, UpdateCaseCategoryCommand command)
    {
        if (id != command.CategoryID)
            return BadRequest(ApiResponse<bool>.FailureResponse("Route ID and body CategoryID do not match."));

        var result = await mediator.Send(command);
        return Ok(ApiResponse<bool>.SuccessResponse(result, "Case category updated successfully."));
    }







    [HttpDelete("{id}")]
    [Authorize(Roles = RoleNames.FirmAdminAndAbove)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await mediator.Send(new DeleteCaseCategoryCommand(id));
        return Ok(ApiResponse<bool>.SuccessResponse(result, "Case category deleted successfully."));
    }
}
