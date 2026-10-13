using LTSBackend.Comman.Responses;
using LTSBackend.Features.Profile.Commands;
using LTSBackend.Features.Profile.DTOs;
using LTSBackend.Features.Profile.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LTSBackend.Features.Profile.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ProfileController> _logger;

    public ProfileController(IMediator mediator, ILogger<ProfileController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }








    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile()
    {
        _logger.LogInformation("Get my profile request");

        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(ApiResponse<bool>.FailureResponse("Invalid or missing user identity."));
        }

        var profile = await _mediator.Send(new GetMyProfileQuery(userId));

        return Ok(ApiResponse<ProfileDTO>.SuccessResponse(profile,"Profile fetched successfully."));
    }













    [HttpPut("me")]
    public async Task<IActionResult> UpdateMyProfile([FromForm] UpdateMyProfileCommand command)
    {
        _logger.LogInformation("Update my profile request");

        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(ApiResponse<bool>.FailureResponse("Invalid or missing user identity."));
        }

        var request = command with { UserID = userId };
        var result = await _mediator.Send(request);

        return Ok(ApiResponse<bool>.SuccessResponse(result, "Profile updated successfully!"));
    }









    [HttpPost("complete/firm-admin")]
    public async Task<IActionResult> CompleteFirmAdminProfile([FromBody] Commands.CompleteFirmAdminProfile.CompleteFirmAdminProfileCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(ApiResponse<ProfileCompletionResultDTO>.SuccessResponse(result, result.Message));
    }






    [HttpPost("complete/firm-user")]
    public async Task<IActionResult> CompleteFirmUserProfile([FromBody] Commands.CompleteFirmUserProfile.CompleteFirmUserProfileCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(ApiResponse<ProfileCompletionResultDTO>.SuccessResponse(result, result.Message));
    }
}
