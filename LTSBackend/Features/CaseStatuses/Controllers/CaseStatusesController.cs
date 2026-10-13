using LTSBackend.Comman.Responses;
using LTSBackend.Features.CaseStatuses.Commands.CreateCaseStatus;
using LTSBackend.Features.CaseStatuses.Commands.DeleteCaseStatus;
using LTSBackend.Features.CaseStatuses.Commands.UpdateCaseStatus;
using LTSBackend.Features.CaseStatuses.DTOs;
using LTSBackend.Features.CaseStatuses.Queries.GetAllCaseStatuses;
using LTSBackend.Features.CaseStatuses.Queries.GetCaseStatusById;
using LTSBackend.Models.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LTSBackend.Features.CaseStatuses.Controllers;







[Route("api/[controller]")]
[ApiController]
[Authorize]
public class CaseStatusesController(IMediator mediator) : ControllerBase
{






    [HttpGet]
    [Authorize(Roles = RoleNames.FirmAdminAndAbove)]
    public async Task<IActionResult> GetAll([FromQuery] string? searchText, [FromQuery] bool activeOnly = true)
    {
        var statuses = await mediator.Send(new GetAllCaseStatusesQuery(searchText, activeOnly));
        return Ok(ApiResponse<List<CaseStatusDTO>>.SuccessResponse(statuses));
    }





    [HttpGet("{id}")]
    [Authorize(Roles = RoleNames.FirmAdminAndAbove)]
    public async Task<IActionResult> GetById(int id)
    {
        var status = await mediator.Send(new GetCaseStatusByIdQuery(id));
        return Ok(ApiResponse<CaseStatusDTO>.SuccessResponse(status));
    }






    [HttpPost]
    [Authorize(Roles = RoleNames.FirmAdminAndAbove)]
    public async Task<IActionResult> Create(CreateCaseStatusCommand command)
    {
        var id = await mediator.Send(command);
        return Ok(ApiResponse<int>.SuccessResponse(id, "Case status created successfully."));
    }






    [HttpPut("{id}")]
    [Authorize(Roles = RoleNames.FirmAdminAndAbove)]
    public async Task<IActionResult> Update(int id, UpdateCaseStatusCommand command)
    {
        if (id != command.StatusID)
            return BadRequest(ApiResponse<bool>.FailureResponse("Route ID and body StatusID do not match."));

        var result = await mediator.Send(command);
        return Ok(ApiResponse<bool>.SuccessResponse(result, "Case status updated successfully."));
    }






    [HttpDelete("{id}")]
    [Authorize(Roles = RoleNames.FirmAdminAndAbove)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await mediator.Send(new DeleteCaseStatusCommand(id));
        return Ok(ApiResponse<bool>.SuccessResponse(result, "Case status deleted successfully."));
    }
}
