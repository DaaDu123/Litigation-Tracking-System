using LTSBackend.Comman.Responses;
using LTSBackend.Features.CaseStages.Commands.CreateCaseStage;
using LTSBackend.Features.CaseStages.Commands.DeleteCaseStage;
using LTSBackend.Features.CaseStages.Commands.UpdateCaseStage;
using LTSBackend.Features.CaseStages.DTOs;
using LTSBackend.Features.CaseStages.Queries.GetAllCaseStages;
using LTSBackend.Features.CaseStages.Queries.GetCaseStageById;
using LTSBackend.Models.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LTSBackend.Features.CaseStages.Controllers;






[Route("api/[controller]")]
[ApiController]
[Authorize]
public class CaseStagesController(IMediator mediator) : ControllerBase
{






    [HttpGet]
    [Authorize(Roles = RoleNames.FirmAdminAndAbove)]
    public async Task<IActionResult> GetAll([FromQuery] string? searchText, [FromQuery] bool activeOnly = true)
    {
        var stages = await mediator.Send(new GetAllCaseStagesQuery(searchText, activeOnly));
        return Ok(ApiResponse<List<CaseStageDTO>>.SuccessResponse(stages));
    }





    [HttpGet("{id}")]
    [Authorize(Roles = RoleNames.FirmAdminAndAbove)]
    public async Task<IActionResult> GetById(int id)
    {
        var stage = await mediator.Send(new GetCaseStageByIdQuery(id));
        return Ok(ApiResponse<CaseStageDTO>.SuccessResponse(stage));
    }






    [HttpPost]
    [Authorize(Roles = RoleNames.FirmAdminAndAbove)]
    public async Task<IActionResult> Create(CreateCaseStageCommand command)
    {
        var id = await mediator.Send(command);
        return Ok(ApiResponse<int>.SuccessResponse(id, "Case stage created successfully."));
    }






    [HttpPut("{id}")]
    [Authorize(Roles = RoleNames.FirmAdminAndAbove)]
    public async Task<IActionResult> Update(int id, UpdateCaseStageCommand command)
    {
        if (id != command.StageID)
            return BadRequest(ApiResponse<bool>.FailureResponse("Route ID and body StageID do not match."));

        var result = await mediator.Send(command);
        return Ok(ApiResponse<bool>.SuccessResponse(result, "Case stage updated successfully."));
    }






    [HttpDelete("{id}")]
    [Authorize(Roles = RoleNames.FirmAdminAndAbove)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await mediator.Send(new DeleteCaseStageCommand(id));
        return Ok(ApiResponse<bool>.SuccessResponse(result, "Case stage deleted successfully."));
    }
}
