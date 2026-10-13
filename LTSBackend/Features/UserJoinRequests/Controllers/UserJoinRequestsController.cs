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











[Route("api/[controller]")]
[ApiController]
public class UserJoinRequestsController(IMediator _mediator, ILogger<UserJoinRequestsController> _logger) : ControllerBase
{




    [HttpGet("firms")]
    [Authorize]
    public async Task<IActionResult> GetJoinableFirms()
    {
        var firms = await _mediator.Send(new GetJoinableFirmsQuery());
        return Ok(ApiResponse<List<JoinableFirmDTO>>.SuccessResponse(firms, "Firms fetched"));
    }







    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Submit([FromBody] SubmitUserJoinRequestCommand command)
    {
        _logger.LogInformation("User join request submitted for firm {FirmId}", command.FirmID);
        var requestId = await _mediator.Send(command);
        return Ok(ApiResponse<int>.SuccessResponse(requestId, "Your request has been submitted. The firm's Admin will review it and you'll be notified by email."));
    }







    [HttpGet("mine")]
    [Authorize]
    public async Task<IActionResult> GetMine()
    {
        var result = await _mediator.Send(new Queries.GetMyJoinRequest.GetMyJoinRequestQuery());
        return Ok(ApiResponse<UserJoinRequestDTO?>.SuccessResponse(result, "Your latest request"));
    }






    [HttpPut("{id}/cancel")]
    [Authorize]
    public async Task<IActionResult> Cancel(int id)
    {
        var result = await _mediator.Send(new CancelUserJoinRequestCommand(id));
        return Ok(ApiResponse<bool>.SuccessResponse(result, "Request cancelled. You can now request a different firm."));
    }







    [HttpGet]
    [Authorize(Roles = RoleNames.FirmAdminOnly)]
    public async Task<IActionResult> GetAll([FromQuery] string? status)
    {
        var requests = await _mediator.Send(new GetUserJoinRequestsQuery(status));
        return Ok(ApiResponse<List<UserJoinRequestDTO>>.SuccessResponse(requests, "Join requests fetched"));
    }






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




    private int? GetActingUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(claim, out var id) ? id : null;
    }
}

public record RejectUserJoinRequestBody(string? Reason);
