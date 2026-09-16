using LTSBackend.Comman.Responses;
using LTSBackend.Features.UserJoinRequests.Commands.ApproveUserJoinRequest;
using LTSBackend.Features.UserJoinRequests.Commands.RejectUserJoinRequest;
using LTSBackend.Features.UserJoinRequests.Commands.SubmitUserJoinRequest;
using LTSBackend.Features.UserJoinRequests.DTOs;
using LTSBackend.Features.UserJoinRequests.Queries.GetJoinableFirms;
using LTSBackend.Features.UserJoinRequests.Queries.GetUserJoinRequests;
using LTSBackend.Models.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace LTSBackend.Features.UserJoinRequests.Controllers;

/// <summary>
/// Self-service "join an existing firm" workflow for Partner / Associate
/// Lawyer / Moharrir / Intern Paralegal - the one tier down from
/// FirmAdminRequestsController. Submission is anonymous; review is
/// restricted to the target firm's own FirmAdmin(s). The FirmAdmin's
/// existing direct "Create User" flow (UsersController) is untouched and
/// still creates users immediately, with no approval step.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class UserJoinRequestsController(IMediator _mediator, ILogger<UserJoinRequestsController> _logger) : ControllerBase
{
    // =====================================================
    // GET JOINABLE FIRMS — Anonymous
    // Minimal public firm picker (FirmID/FirmName/FirmCode only) so the
    // join-request form can offer a dropdown instead of requiring the
    // requester to already know a firm code by heart.
    // =====================================================
    [HttpGet("firms")]
    [AllowAnonymous]
    public async Task<IActionResult> GetJoinableFirms()
    {
        var firms = await _mediator.Send(new GetJoinableFirmsQuery());
        return Ok(ApiResponse<List<JoinableFirmDTO>>.SuccessResponse(firms, "Firms fetched"));
    }

    // =====================================================
    // SUBMIT USER JOIN REQUEST — Anonymous
    // Lets someone request to join an existing firm as Partner / Associate
    // Lawyer / Moharrir / Intern Paralegal. Goes into a pending queue for
    // that firm's FirmAdmin to review - does not create a user by itself.
    // SECURITY: rate limited ("auth-moderate", see Program.cs) since this
    // is an anonymous, self-service endpoint.
    // =====================================================
    [HttpPost]
    [AllowAnonymous]
    [EnableRateLimiting("auth-moderate")]
    public async Task<IActionResult> Submit([FromBody] SubmitUserJoinRequestCommand command)
    {
        _logger.LogInformation("User join request submitted for firm {FirmId}, role {RoleId}", command.FirmID, command.RequestedRoleID);
        var requestId = await _mediator.Send(command);
        return Ok(ApiResponse<int>.SuccessResponse(requestId,"Your request has been submitted. The firm's Admin will review it and you'll be notified by email."));
    }

    // =====================================================
    // GET ALL USER JOIN REQUESTS — FirmAdmin ONLY
    // Lists pending/approved/rejected join requests (optionally filtered
    // by status) aimed at the acting FirmAdmin's own firm. Cross-firm
    // isolation is enforced by the UserJoinRequest tenant query filter
    // in AppDbContext, not by anything in this controller.
    //
    // Deliberately FirmAdmin-only (not RoleNames.FirmAdminAndAbove): per
    // policy, Partner has ZERO access to the Join Request queue - this
    // closes off the approval path as another route by which Partner
    // could otherwise end up creating a user, matching Approve below.
    // =====================================================
    [HttpGet]
    [Authorize(Roles = RoleNames.FirmAdminOnly)]
    public async Task<IActionResult> GetAll([FromQuery] string? status)
    {
        var requests = await _mediator.Send(new GetUserJoinRequestsQuery(status));
        return Ok(ApiResponse<List<UserJoinRequestDTO>>.SuccessResponse(requests, "Join requests fetched"));
    }

    // =====================================================
    // APPROVE USER JOIN REQUEST — FirmAdmin ONLY
    // Accepts a pending request: activates the requested Partner/Associate/
    // Moharrir/Intern account, linked to this firm with the requested role.
    // This action creates/activates a brand-new user account, exactly like
    // UsersController.Create - Partner has no access to either.
    // =====================================================
    [HttpPut("{id}/approve")]
    [Authorize(Roles = RoleNames.FirmAdminOnly)]
    public async Task<IActionResult> Approve(int id)
    {
        var actingUserId = GetActingUserId();
        if (actingUserId == null)
            return Unauthorized(ApiResponse<int>.FailureResponse("Invalid identity."));

        var userId = await _mediator.Send(new ApproveUserJoinRequestCommand(id) { ActingUserID = actingUserId.Value });
        return Ok(ApiResponse<int>.SuccessResponse(userId, "Request approved - user account created and activated."));
    }

    // =====================================================
    // REJECT USER JOIN REQUEST — FirmAdmin ONLY
    // Declines a pending request, optionally with a reason, without
    // creating a user account. Kept FirmAdmin-only alongside Approve/GetAll
    // above so the entire Join Request queue - view, approve, and reject -
    // is exclusively the FirmAdmin's call; Partner has no access to any of
    // the three.
    // =====================================================
    [HttpPut("{id}/reject")]
    [Authorize(Roles = RoleNames.FirmAdminOnly)]
    public async Task<IActionResult> Reject(int id, [FromBody] RejectUserJoinRequestBody? body)
    {
        var actingUserId = GetActingUserId();
        if (actingUserId == null)
            return Unauthorized(ApiResponse<bool>.FailureResponse("Invalid identity."));

        var result = await _mediator.Send(new RejectUserJoinRequestCommand(id, body?.Reason) { ActingUserID = actingUserId.Value });
        return Ok(ApiResponse<bool>.SuccessResponse(result, "Request rejected."));
    }

    // =====================================================
    // GET ACTING USER ID — internal helper, not an endpoint
    // Reads the caller's own UserID from their JWT claim so Approve/Reject
    // can stamp who acted, without ever trusting a user ID from the
    // request body.
    // =====================================================
    private int? GetActingUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(claim, out var id) ? id : null;
    }
}

public record RejectUserJoinRequestBody(string? Reason);
