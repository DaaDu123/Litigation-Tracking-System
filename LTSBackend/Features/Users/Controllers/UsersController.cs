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
    // =====================================================
    // CREATE USER — FirmAdmin ONLY (deliberate exception — Partner excluded)
    // Registers a brand-new user (Partner / Associate Lawyer / Moharrir /
    // Intern Paralegal, etc.) inside the acting FirmAdmin's own firm, or a
    // new firm-scoped account created by a SuperAdmin. Accepts multipart
    // form data because the create form can also carry a profile photo.
    // The acting user's ID is pulled from the JWT claim (never trusted from
    // the request body) and stamped onto the command as ActingUserID so the
    // handler can enforce role-hierarchy and firm-scoping rules.
    //
    // Uses RoleNames.FirmAdminOnly (NOT FirmAdminAndAbove) on purpose: per
    // policy, Partner has FirmAdmin-equivalent access everywhere else in
    // this controller, EXCEPT creating a new user account. That one action
    // stays FirmAdmin-exclusive.
    // =====================================================
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

    // =====================================================
    // GET ALL USERS — FirmAdmin and Partner (view only)
    // Returns the full, firm-scoped list of active users so admin/partner
    // screens (user grids, case-assignment dropdowns, etc.) can populate.
    // Row level scoping to the caller's firm is enforced inside the query
    // handler, not here.
    //
    // Partner can SELECT/VIEW users (needed e.g. to pick a lawyer when
    // assigning a case) but has NO Create/Update/Delete access - see those
    // three actions below, all RoleNames.FirmAdminOnly. Join Requests are a
    // separate, stricter story: Partner has ZERO access there (not even
    // view) - see UserJoinRequestsController.
    // =====================================================
    [HttpGet]
    [Authorize(Roles = RoleNames.PartnerAndAbove)]
    public async Task<IActionResult> GetAll([FromQuery] string? search)
    {
        _logger.LogInformation("Get all users request (search: {Search})", search ?? "(none)");

        var users = await _mediator.Send(new GetAllUsersQuery(search));

        return Ok(ApiResponse<List<UserDTO>>.SuccessResponse(users, "Users successfully fetched"));
    }

    // =====================================================
    // GET USER BY ID — FirmAdmin and Partner (view only)
    // Fetches a single user's full profile/detail record by their UserID.
    // Used by user-detail screens and by CreatedAtAction on user creation.
    // Returns 404 if no such user exists (or isn't visible to this firm).
    // =====================================================
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

    // =====================================================
    // NOTE: there is deliberately no generic "Update User" endpoint here.
    // By policy, nobody may change another person's profile (name,
    // contact info, or photo) — that's only ever editable by the user
    // themselves via PUT /api/profile/me (ProfileController). A Firm
    // Admin's only levers over an existing user are role
    // (PUT /api/users/{id}/role, below) and lifecycle status
    // (activate/deactivate/block/unblock/remove/permanent-delete, all
    // FirmAdminOnly, all below). This mirrors the frontend, which has no
    // "Edit User" screen anymore either.
    // =====================================================

    // =====================================================
    // DELETE USER (Deactivate — reversible) — FirmAdmin ONLY
    // Soft-deletes/deactivates a user so they can no longer log in, without
    // erasing their historical case/hearing/document records. This is
    // reversible via the Activate endpoint below. Distinct from
    // PermanentDelete, which removes the record outright.
    // =====================================================
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

    // =====================================================
    // ACTIVATE USER (reverses Deactivate) — FirmAdmin ONLY
    // Re-enables a previously deactivated user so they can log in again.
    // Does not touch a permanently-deleted user — once permanently deleted,
    // a user cannot be reactivated through this endpoint.
    // =====================================================
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

    // =====================================================
    // PERMANENT DELETE — FirmAdmin ONLY
    // Hard-deletes a (typically already-deactivated) user record from the
    // system. Unlike Delete/Activate above, this is NOT reversible from the
    // application — the row is gone. Used for cleaning up test accounts or
    // records a firm no longer wants retained at all.
    // =====================================================
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

    // =====================================================
    // GET DELETED USERS (email-reuse candidates) — SuperAdmin only
    // NEW: part of the Reuse/Soft-Delete/Email-Ownership fix. Lists every
    // soft-deleted user across every firm so a SuperAdmin can find the
    // record to release when a different firm needs to reclaim that email.
    // =====================================================
    [HttpGet("deleted")]
    [Authorize(Roles = RoleNames.SuperAdminOnly)]
    public async Task<IActionResult> GetDeleted()
    {
        _logger.LogInformation("Get deleted users request");

        var users = await _mediator.Send(new GetDeletedUsersQuery());

        return Ok(ApiResponse<List<DeletedUserDTO>>.SuccessResponse(users, "Deleted users successfully fetched"));
    }

    // =====================================================
    // RELEASE EMAIL FOR REASSIGNMENT — SuperAdmin only
    // NEW: part of the Reuse/Soft-Delete/Email-Ownership fix. A deleted
    // user's email is reserved for their original firm by default (see
    // CreateUserCommandHandler); this explicitly releases it so ANY firm
    // can reuse that record's email going forward. Deliberately a
    // platform-level decision, not something the original FirmAdmin can
    // do unilaterally.
    // =====================================================
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

    // =====================================================
    // GET MY PROFILE — Any authenticated user
    // Lets the currently logged-in user fetch their own profile (name,
    // email, role, photo, etc.) for the "My Profile" screen, without
    // needing the Partner-and-above permission required by GetById. The
    // user ID is read from the caller's own JWT claim, so a user can never
    // use this endpoint to view someone else's profile.
    // =====================================================
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

    // =====================================================
    // BLOCK FIRM USER — FirmAdmin ONLY
    // Locks a firm user out of the firm entirely (reversible via Unblock).
    // =====================================================
    [HttpPut("{id}/block")]
    [Authorize(Roles = RoleNames.FirmAdminOnly)]
    public async Task<IActionResult> Block(int id, [FromBody] BlockUserBody body)
    {
        var result = await _mediator.Send(new Commands.BlockFirmUser.BlockFirmUserCommand(id, body.Reason));
        return Ok(ApiResponse<bool>.SuccessResponse(result, "User blocked."));
    }

    // =====================================================
    // UNBLOCK FIRM USER — FirmAdmin ONLY
    // =====================================================
    [HttpPut("{id}/unblock")]
    [Authorize(Roles = RoleNames.FirmAdminOnly)]
    public async Task<IActionResult> Unblock(int id)
    {
        var result = await _mediator.Send(new Commands.UnblockFirmUser.UnblockFirmUserCommand(id));
        return Ok(ApiResponse<bool>.SuccessResponse(result, "User unblocked."));
    }

    // =====================================================
    // GET BLOCKED FIRM USERS — FirmAdmin ONLY
    // =====================================================
    [HttpGet("blocked")]
    [Authorize(Roles = RoleNames.FirmAdminOnly)]
    public async Task<IActionResult> GetBlocked()
    {
        var result = await _mediator.Send(new Queries.GetBlockedFirmUsers.GetBlockedFirmUsersQuery());
        return Ok(ApiResponse<List<Users.DTOs.BlockedFirmUserDTO>>.SuccessResponse(result, "Blocked users fetched"));
    }

    // =====================================================
    // REMOVE FIRM USER — FirmAdmin ONLY
    // Detaches the user from the firm entirely (distinct from Block) -
    // a mandatory reason is required and shown to the removed user.
    // =====================================================
    [HttpPut("{id}/remove")]
    [Authorize(Roles = RoleNames.FirmAdminOnly)]
    public async Task<IActionResult> Remove(int id, [FromBody] RemoveUserBody body)
    {
        var result = await _mediator.Send(new Commands.RemoveFirmUser.RemoveFirmUserCommand(id, body.Reason));
        return Ok(ApiResponse<bool>.SuccessResponse(result, "User removed from the firm."));
    }

    // =====================================================
    // CHANGE FIRM USER ROLE — FirmAdmin ONLY
    // =====================================================
    [HttpPut("{id}/role")]
    [Authorize(Roles = RoleNames.FirmAdminOnly)]
    public async Task<IActionResult> ChangeRole(int id, [FromBody] ChangeRoleBody body)
    {
        var result = await _mediator.Send(new Commands.ChangeUserRole.ChangeUserRoleCommand(id, body.NewRoleID));
        return Ok(ApiResponse<bool>.SuccessResponse(result, "Role updated."));
    }

    // =====================================================
    // SET MY AVAILABILITY — FirmAdmin ONLY
    // Firm Admin sets their own Active/Inactive status.
    // =====================================================
    [HttpPut("me/availability")]
    [Authorize(Roles = RoleNames.FirmAdminOnly)]
    public async Task<IActionResult> SetAvailability([FromBody] Commands.SetAvailability.SetAvailabilityCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(ApiResponse<bool>.SuccessResponse(result, command.IsAvailable ? "You are now Active." : "You are now Inactive."));
    }

    // =====================================================
    // GET MY FIRM ADMIN'S AVAILABILITY — Any authenticated firm user
    // =====================================================
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
