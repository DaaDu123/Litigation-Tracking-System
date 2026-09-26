using LTSBackend.Comman.Responses;
using LTSBackend.Features.FirmAdminRequests.Commands.ApproveFirmAdminRequest;
using LTSBackend.Features.FirmAdminRequests.Commands.RejectFirmAdminRequest;
using LTSBackend.Features.FirmAdminRequests.Commands.SubmitFirmAdminRequest;
using LTSBackend.Features.FirmAdminRequests.Commands.SubmitFirmAdminRequestFromAccount;
using LTSBackend.Features.FirmAdminRequests.DTOs;
using LTSBackend.Features.FirmAdminRequests.Queries.GetFirmAdminRequests;
using LTSBackend.Features.FirmAdminRequests.Queries.GetMyFirmAdminRequest;
using LTSBackend.Models.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace LTSBackend.Features.FirmAdminRequests.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FirmAdminRequestsController(IMediator _mediator, ILogger<FirmAdminRequestsController> _logger) : ControllerBase
{
    // =====================================================
    // SUBMIT FIRM ADMIN REQUEST — Anonymous
    // Lets someone request that a new firm workspace + FirmAdmin account
    // be created for their firm. Goes into a pending queue for a
    // SuperAdmin to review — does not create anything by itself.
    // SECURITY: rate limited ("auth-moderate", see Program.cs) since this
    // is an anonymous, self-service endpoint.
    // =====================================================
    [HttpPost]
    [AllowAnonymous]
    [EnableRateLimiting("auth-moderate")]
    public async Task<IActionResult> Submit([FromBody] SubmitFirmAdminRequestCommand command)
    {
        _logger.LogInformation("Firm Admin request submitted for email: {Email}", command.Email);
        var requestId = await _mediator.Send(command);
        return Ok(ApiResponse<int>.SuccessResponse(requestId, "Your request has been submitted. A Super Admin will review it and you'll be notified by email."));
    }

    // =====================================================
    // SUBMIT FIRM ADMIN REQUEST FROM ACCOUNT — Authenticated
    // "Create Firm" from an already-registered user's own dashboard
    // sidebar. No email/password collected here - the request is linked
    // straight to the caller's existing account, so approval later
    // promotes that SAME account instead of creating a duplicate one.
    // Does NOT require the user to register again with the same email.
    // =====================================================
    [HttpPost("from-account")]
    [Authorize]
    public async Task<IActionResult> SubmitFromAccount()
    {
        var requestId = await _mediator.Send(new SubmitFirmAdminRequestFromAccountCommand());
        return Ok(ApiResponse<int>.SuccessResponse(requestId, "Your request has been submitted. A Super Admin will review it and you'll be notified by email."));
    }

    // =====================================================
    // GET MY FIRM ADMIN REQUEST — Authenticated
    // The caller's own latest "Create Firm" request (any status),
    // submitted from their account, for the sidebar modal's
    // pending/rejected state. Returns null (200, no data) if they've
    // never submitted one this way.
    // =====================================================
    [HttpGet("mine")]
    [Authorize]
    public async Task<IActionResult> GetMine()
    {
        var result = await _mediator.Send(new GetMyFirmAdminRequestQuery());
        return Ok(ApiResponse<FirmAdminRequestDTO?>.SuccessResponse(result, "Your latest request"));
    }

    // =====================================================
    // GET ALL FIRM ADMIN REQUESTS — SuperAdmin only
    // Lists pending/approved/rejected firm-admin requests (optionally
    // filtered by status) so a SuperAdmin can review the queue.
    // =====================================================
    [HttpGet]
    [Authorize(Roles = RoleNames.SuperAdminOnly)]
    public async Task<IActionResult> GetAll([FromQuery] string? status)
    {
        var requests = await _mediator.Send(new GetFirmAdminRequestsQuery(status));
        return Ok(ApiResponse<List<FirmAdminRequestDTO>>.SuccessResponse(requests, "Firm Admin requests fetched"));
    }

    // =====================================================
    // APPROVE FIRM ADMIN REQUEST — SuperAdmin only
    // Accepts a pending request: provisions the new firm workspace and
    // creates its first FirmAdmin account in one step.
    // =====================================================
    [HttpPut("{id}/approve")]
    [Authorize(Roles = RoleNames.SuperAdminOnly)]
    public async Task<IActionResult> Approve(int id)
    {
        var actingUserId = GetActingUserId();
        if (actingUserId == null)
            return Unauthorized(ApiResponse<int>.FailureResponse("Invalid identity."));

        var firmId = await _mediator.Send(new ApproveFirmAdminRequestCommand(id) { ActingUserID = actingUserId.Value });
        return Ok(ApiResponse<int>.SuccessResponse(firmId, "Request approved - firm workspace and admin account created."));
    }

    // =====================================================
    // REJECT FIRM ADMIN REQUEST — SuperAdmin only
    // Declines a pending request, optionally with a reason, without
    // creating a firm workspace.
    // =====================================================
    [HttpPut("{id}/reject")]
    [Authorize(Roles = RoleNames.SuperAdminOnly)]
    public async Task<IActionResult> Reject(int id, [FromBody] RejectFirmAdminRequestBody? body)
    {
        var actingUserId = GetActingUserId();
        if (actingUserId == null)
            return Unauthorized(ApiResponse<bool>.FailureResponse("Invalid identity."));

        var result = await _mediator.Send(new RejectFirmAdminRequestCommand(id, body?.Reason) { ActingUserID = actingUserId.Value });
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

public record RejectFirmAdminRequestBody(string? Reason);