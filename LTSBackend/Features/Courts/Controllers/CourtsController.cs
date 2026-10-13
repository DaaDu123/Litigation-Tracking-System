using LTSBackend.Comman.Responses;
using LTSBackend.Features.Courts.Commands.CreateCourt;
using LTSBackend.Features.Courts.Commands.DeleteCourt;
using LTSBackend.Features.Courts.Commands.UpdateCourt;
using LTSBackend.Features.Courts.DTOs;
using LTSBackend.Features.Courts.Queries.GetAllCourts;
using LTSBackend.Features.Courts.Queries.GetCourtById;
using LTSBackend.Models.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LTSBackend.Features.Courts.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class CourtsController(IMediator mediator) : ControllerBase
{









    [HttpGet]
    [Authorize(Roles = RoleNames.FirmAdminAndAbove)]
    public async Task<IActionResult> GetAll([FromQuery] string? searchText,[FromQuery] bool activeOnly = true)
    {
        var courts = await mediator.Send(new GetAllCourtsQuery(searchText, activeOnly));
        return Ok(ApiResponse<List<CourtDTO>>.SuccessResponse(courts));
    }





    [HttpGet("{id}")]
    [Authorize(Roles = RoleNames.FirmAdminAndAbove)]
    public async Task<IActionResult> GetById(int id)
    {
        var court = await mediator.Send(new GetCourtByIdQuery(id));
        return Ok(ApiResponse<CourtDTO>.SuccessResponse(court));
    }










    [HttpPost]
    [Authorize(Roles = RoleNames.FirmAdminAndAbove)]
    public async Task<IActionResult> Create(CreateCourtCommand command)
    {
        var id = await mediator.Send(command);
        return Ok(ApiResponse<int>.SuccessResponse(id, "Court created successfully."));
    }







    [HttpPut("{id}")]
    [Authorize(Roles = RoleNames.FirmAdminAndAbove)]
    public async Task<IActionResult> Update(int id, UpdateCourtCommand command)
    {
        if (id != command.CourtID)
            return BadRequest(ApiResponse<bool>.FailureResponse("Route ID and body CourtID do not match."));

        var result = await mediator.Send(command);
        return Ok(ApiResponse<bool>.SuccessResponse(result, "Court updated successfully."));
    }







    [HttpDelete("{id}")]
    [Authorize(Roles = RoleNames.FirmAdminAndAbove)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await mediator.Send(new DeleteCourtCommand(id));
        return Ok(ApiResponse<bool>.SuccessResponse(result, "Court deleted successfully."));
    }
}
