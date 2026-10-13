using LTSBackend.Comman.Responses;
using LTSBackend.Features.Users.Commands.ActivateUser;
using LTSBackend.Features.Users.Commands.CreateUser;
using LTSBackend.Features.Users.Commands.DeleteUser;
using LTSBackend.Features.Users.Commands.PermanentDeleteUser;
using LTSBackend.Features.Users.Commands.ReleaseUserEmail;
using LTSBackend.Features.Users.DTOs;
using LTSBackend.Features.Users.Queries.GetAllUsers;
using LTSBackend.Features.Users.Queries.GetDeletedUsers;
using LTSBackend.Features.Users.Queries.GetUserById;
using LTSBackend.Models.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LTSBackend.Features.Users.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class UsersController(IMediator _mediator, ILogger<UsersController> _logger) : ControllerBase
{















    [HttpPost]
    [Authorize(Roles = RoleNames.FirmAdminOnly)]
    public async Task<IActionResult> Create([FromForm] CreateUserCommand command)
    {
        var actingUserIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(actingUserIdClaim, out var actingUserId))
            return Unauthorized(ApiResponse<int>.FailureResponse("Invalid identity."));

        var id = await _mediator.Send(command with { ActingUserID = actingUserId });

        return CreatedAtAction(nameof(GetById), new { id },
            ApiResponse<int>.SuccessResponse(id, "User successfully created"));
    }














    [HttpGet]
    [Authorize(Roles = RoleNames.PartnerAndAbove)]
    public async Task<IActionResult> GetAll([FromQuery] string? search)
    {
        _logger.LogInformation("Get all users request (search: {Search})", search ?? "(none)");

        var users = await _mediator.Send(new GetAllUsersQuery(search));

        return Ok(ApiResponse<List<UserDTO>>.SuccessResponse(users, "Users successfully fetched"));
    }







    [HttpGet("{id}")]
    [Authorize(Roles = RoleNames.PartnerAndAbove)]
    public async Task<IActionResult> GetById(int id)
    {
        _logger.LogInformation("Get user request: {UserID}", id);

        var user = await _mediator.Send(new GetUserByIdQuery(id));

        if (user == null)
            return NotFound(ApiResponse<UserDTO>.FailureResponse("User not found"));

        return Ok(ApiResponse<UserDTO>.SuccessResponse(user, "User successfully fetched"));
    }




















    [HttpDelete("{id}")]
    [Authorize(Roles = RoleNames.FirmAdminOnly)]
    public async Task<IActionResult> Delete(int id)
    {
        _logger.LogInformation("Deactivate user request: {UserID}", id);

        var actingUserIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(actingUserIdClaim, out var actingUserId))
            return Unauthorized(ApiResponse<bool>.FailureResponse("Invalid identity."));

        var result = await _mediator.Send(new DeleteUserCommand(id) { ActingUserID = actingUserId });

        return Ok(ApiResponse<bool>.SuccessResponse(result, "User successfully deactivated"));
    }







    [HttpPut("{id}/activate")]
    [Authorize(Roles = RoleNames.FirmAdminOnly)]
    public async Task<IActionResult> Activate(int id)
    {
        _logger.LogInformation("Activate user request: {UserID}", id);

        var actingUserIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(actingUserIdClaim, out var actingUserId))
            return Unauthorized(ApiResponse<bool>.FailureResponse("Invalid identity."));

        var result = await _mediator.Send(new ActivateUserCommand(id) { ActingUserID = actingUserId });

        return Ok(ApiResponse<bool>.SuccessResponse(result, "User successfully activated"));
    }








    [HttpDelete("{id}/permanent")]
    [Authorize(Roles = RoleNames.FirmAdminOnly)]
    public async Task<IActionResult> PermanentDelete(int id)
    {
        _logger.LogInformation("Permanent delete user request: {UserID}", id);

        var actingUserIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(actingUserIdClaim, out var actingUserId))
            return Unauthorized(ApiResponse<bool>.FailureResponse("Invalid identity."));

        var result = await _mediator.Send(new PermanentDeleteUserCommand(id) { ActingUserID = actingUserId });

        return Ok(ApiResponse<bool>.SuccessResponse(result, "User permanently deleted"));
    }







    [HttpGet("deleted")]
    [Authorize(Roles = RoleNames.SuperAdminOnly)]
    public async Task<IActionResult> GetDeleted()
    {
        _logger.LogInformation("Get deleted users request");

        var users = await _mediator.Send(new GetDeletedUsersQuery());

        return Ok(ApiResponse<List<DeletedUserDTO>>.SuccessResponse(users, "Deleted users successfully fetched"));
    }










    [HttpPut("{id}/release")]
    [Authorize(Roles = RoleNames.SuperAdminOnly)]
    public async Task<IActionResult> Release(int id)
    {
        _logger.LogInformation("Release user email request: {UserID}", id);

        var actingUserIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(actingUserIdClaim, out var actingUserId))
            return Unauthorized(ApiResponse<bool>.FailureResponse("Invalid identity."));

        var result = await _mediator.Send(new ReleaseUserEmailCommand(id) { ActingUserID = actingUserId });

        return Ok(ApiResponse<bool>.SuccessResponse(result, "Email released for reassignment"));
    }









    [HttpGet("profile/me")]
    public async Task<IActionResult> GetMyProfile()
    {
        _logger.LogInformation("Get my profile request");

        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized(ApiResponse<bool>.FailureResponse("Invalid or missing user identity"));

        var user = await _mediator.Send(new GetUserByIdQuery(userId));

        if (user == null)
        {
            return NotFound(ApiResponse<UserDTO>.FailureResponse("User not found"));
        }

        return Ok(ApiResponse<UserDTO>.SuccessResponse(user, "Profile successfully fetched"));
    }





    [HttpPut("{id}/block")]
    [Authorize(Roles = RoleNames.FirmAdminOnly)]
    public async Task<IActionResult> Block(int id, [FromBody] BlockUserBody body)
    {
        var result = await _mediator.Send(new Commands.BlockFirmUser.BlockFirmUserCommand(id, body.Reason));
        return Ok(ApiResponse<bool>.SuccessResponse(result, "User blocked."));
    }




    [HttpPut("{id}/unblock")]
    [Authorize(Roles = RoleNames.FirmAdminOnly)]
    public async Task<IActionResult> Unblock(int id)
    {
        var result = await _mediator.Send(new Commands.UnblockFirmUser.UnblockFirmUserCommand(id));
        return Ok(ApiResponse<bool>.SuccessResponse(result, "User unblocked."));
    }




    [HttpGet("blocked")]
    [Authorize(Roles = RoleNames.FirmAdminOnly)]
    public async Task<IActionResult> GetBlocked()
    {
        var result = await _mediator.Send(new Queries.GetBlockedFirmUsers.GetBlockedFirmUsersQuery());
        return Ok(ApiResponse<List<Users.DTOs.BlockedFirmUserDTO>>.SuccessResponse(result, "Blocked users fetched"));
    }






    [HttpPut("{id}/remove")]
    [Authorize(Roles = RoleNames.FirmAdminOnly)]
    public async Task<IActionResult> Remove(int id, [FromBody] RemoveUserBody body)
    {
        var result = await _mediator.Send(new Commands.RemoveFirmUser.RemoveFirmUserCommand(id, body.Reason));
        return Ok(ApiResponse<bool>.SuccessResponse(result, "User removed from the firm."));
    }




    [HttpPut("{id}/role")]
    [Authorize(Roles = RoleNames.FirmAdminOnly)]
    public async Task<IActionResult> ChangeRole(int id, [FromBody] ChangeRoleBody body)
    {
        var result = await _mediator.Send(new Commands.ChangeUserRole.ChangeUserRoleCommand(id, body.NewRoleID));
        return Ok(ApiResponse<bool>.SuccessResponse(result, "Role updated."));
    }





    [HttpPut("me/availability")]
    [Authorize(Roles = RoleNames.FirmAdminOnly)]
    public async Task<IActionResult> SetAvailability([FromBody] Commands.SetAvailability.SetAvailabilityCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(ApiResponse<bool>.SuccessResponse(result, command.IsAvailable ? "You are now Active." : "You are now Inactive."));
    }




    [HttpGet("firm-admin/availability")]
    public async Task<IActionResult> GetFirmAdminAvailability()
    {
        var result = await _mediator.Send(new Queries.GetFirmAdminAvailability.GetFirmAdminAvailabilityQuery());
        return Ok(ApiResponse<Users.DTOs.FirmAdminAvailabilityDTO>.SuccessResponse(result, "Availability fetched"));
    }
}

public record BlockUserBody(string Reason);
public record RemoveUserBody(string Reason);
public record ChangeRoleBody(int NewRoleID);
