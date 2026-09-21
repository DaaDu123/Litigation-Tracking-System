using LTSBackend.Comman.Responses;
using LTSBackend.Features.UserJoinRequests.Commands.ApproveUserJoinRequest;
using LTSBackend.Features.UserJoinRequests.Commands.CancelUserJoinRequest;
using LTSBackend.Features.UserJoinRequests.Commands.RejectUserJoinRequest;
using LTSBackend.Features.UserJoinRequests.Commands.SubmitUserJoinRequest;
using LTSBackend.Features.UserJoinRequests.DTOs;
using LTSBackend.Features.UserJoinRequests.Queries.GetJoinableFirms;
using LTSBackend.Features.UserJoinRequests.Queries.GetUserJoinRequests;
using LTSBackend.Models.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LTSBackend.Features.UserJoinRequests.Controllers;

/// <summary>
/// Self-service "join an existing firm" workflow for Partner / Associate
/// Lawyer / Moharrir / Intern Paralegal. Registration (RegisterCommand)
/// and this join-request are two separate steps: the requester already
/// has an account (Email + Password) and a completed profile by the time
/// they reach here - see AuthController.Register and
/// CompleteFirmUserProfileCommand. Submission requires [Authorize]
/// (any authenticated user); review is restricted to the target firm's
/// own FirmAdmin.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class UserJoinRequestsController(IMediator _mediator, ILogger<UserJoinRequestsController> _logger) : ControllerBase
{
    // =====================================================
    // GET JOINABLE FIRMS — Authenticated (any role, incl. no-firm-yet users)
    // "Available Firm Admins" directory for the requester's own dashboard.
    // =====================================================
    [HttpGet("firms")]
    [Authorize]
    public async Task<IActionResult> GetJoinableFirms()
    {
        var firms = await _mediator.Send(new GetJoinableFirmsQuery());
        return Ok(ApiResponse<List<JoinableFirmDTO>>.SuccessResponse(firms, "Firms fetched"));
    }

    // =====================================================
    // SUBMIT USER JOIN REQUEST — Authenticated
    // Sends ONE request to join a firm. All eligibility checks (profile
    // completed, not already a firm member, no other pending request,
    // not blocked from this firm) are enforced in the handler, not here.
    // =====================================================
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Submit([FromBody] SubmitUserJoinRequestCommand command)
    {
        _logger.LogInformation("User join request submitted for firm {FirmId}", command.FirmID);
        var requestId = await _mediator.Send(command);
        return Ok(ApiResponse<int>.SuccessResponse(requestId, "Your request has been submitted. The firm's Admin will review it and you'll be notified by email."));
    }

    // =====================================================
    // GET MY JOIN REQUEST — Authenticated
    // The requester's own latest request (any status) for their "Request
    // Pending / Cancel Request" dashboard card. Returns null (200, no
    // data) if they've never submitted one.
    // =====================================================
    [HttpGet("mine")]
    [Authorize]
    public async Task<IActionResult> GetMine()
    {
        var result = await _mediator.Send(new Queries.GetMyJoinRequest.GetMyJoinRequestQuery());
        return Ok(ApiResponse<UserJoinRequestDTO?>.SuccessResponse(result, "Your latest request"));
    }

    // =====================================================
    // CANCEL MY JOIN REQUEST — Authenticated
    // Lets the requester cancel their own still-pending request, freeing
    // them to request a different firm right away.
    // =====================================================
    [HttpPut("{id}/cancel")]
    [Authorize]
    public async Task<IActionResult> Cancel(int id)
    {
        var result = await _mediator.Send(new CancelUserJoinRequestCommand(id));
        return Ok(ApiResponse<bool>.SuccessResponse(result, "Request cancelled. You can now request a different firm."));
    }

    // =====================================================
    // GET ALL USER JOIN REQUESTS — FirmAdmin ONLY
    // Lists pending/approved/rejected/cancelled join requests aimed at the
    // acting FirmAdmin's own firm. Cross-firm isolation is enforced by
    // the UserJoinRequest tenant query filter in AppDbContext.
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
    // Attaches the already-registered requester to this firm as
    // Intern/Paralegal (the always-default role on acceptance).
    // =====================================================
    [HttpPut("{id}/approve")]
    [Authorize(Roles = RoleNames.FirmAdminOnly)]
    public async Task<IActionResult> Approve(int id)
    {
        var actingUserId = GetActingUserId();
        if (actingUserId == null)
            return Unauthorized(ApiResponse<int>.FailureResponse("Invalid identity."));

        var userId = await _mediator.Send(new ApproveUserJoinRequestCommand(id) { ActingUserID = actingUserId.Value });
        return Ok(ApiResponse<int>.SuccessResponse(userId, "Request approved - user added to your firm as Intern/Paralegal."));
    }

    // =====================================================
    // REJECT USER JOIN REQUEST — FirmAdmin ONLY
    // Declines a pending request, optionally with a reason. The
    // requester's own account is untouched and free to request elsewhere.
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
    // =====================================================
    private int? GetActingUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(claim, out var id) ? id : null;
    }
}

public record RejectUserJoinRequestBody(string? Reason);
