using LTSBackend.Comman.Responses;
using LTSBackend.Features.Authorization;
using LTSBackend.Features.LoginHistory.DTOs;
using LTSBackend.Features.LoginHistory.GetAllLoginHistory;
using LTSBackend.Features.LoginHistory.GetMyLoginHistory;
using LTSBackend.Features.LoginHistory.Queries.GetAllLoginHistory;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LTSBackend.Features.LoginHistory.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LoginHistoryController(IMediator mediator) : ControllerBase
{





    [HttpGet]
    [HasPermission("ViewLoginHistory")]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] string? status,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await mediator.Send(new GetAllLoginHistoryQuery(search, fromDate, toDate, status, pageNumber, pageSize));
        return Ok(ApiResponse<PagedResult<LoginHistoryDTO>>.SuccessResponse(result, "Login history fetched successfully."));
    }







    [HttpGet("my")]
    public async Task<IActionResult> MyHistory()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(ApiResponse<bool>.FailureResponse("Invalid user identity"));
        }

        var result = await mediator.Send(new GetMyLoginHistoryQuery(userId));

        return Ok(ApiResponse<List<MyLoginHistoryDTO>>.SuccessResponse(result, "My login history fetched successfully."));
    }
}
