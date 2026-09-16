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

    // =====================================================
    // GET MY PROFILE — Any authenticated user
    // Returns the currently logged-in user's own profile (name, email,
    // phone, designation, photo, etc.). The user ID always comes from
    // their own JWT claim, so a user can never fetch someone else's
    // profile through this endpoint.
    // =====================================================
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

    // =====================================================
    // UPDATE MY PROFILE — Any authenticated user
    // Lets a user edit their own profile fields (name, contact info,
    // photo, etc.). Per SRS FR-19, a user cannot change their own role
    // through this endpoint — role changes are FirmAdmin/SuperAdmin-only,
    // handled elsewhere (UsersController.Update).
    // =====================================================
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

        return Ok(ApiResponse<bool>.SuccessResponse(result,"Profile updated successfully!"));
    }
}
