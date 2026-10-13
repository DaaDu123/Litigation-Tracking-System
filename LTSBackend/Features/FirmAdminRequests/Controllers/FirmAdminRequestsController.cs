using LTSBackend.Comman.Responses;
using LTSBackend.Features.FirmAdminRequests.Commands.ApproveFirmAdminRequest;
using LTSBackend.Features.FirmAdminRequests.Commands.CancelFirmAdminRequestFromAccount;
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








    [HttpPost]
    [AllowAnonymous]
    [EnableRateLimiting("auth-moderate")]
    public async Task<IActionResult> Submit([FromBody] SubmitFirmAdminRequestCommand command)
    {
        _logger.LogInformation("Firm Admin request submitted for email: {Email}", command.Email);
        var requestId = await _mediator.Send(command);
        return Ok(ApiResponse<int>.SuccessResponse(requestId, "Your request has been submitted. A Super Admin will review it and you'll be notified by email."));
    }









    [HttpPost("from-account")]
    [Authorize]
    public async Task<IActionResult> SubmitFromAccount()
    {
        var requestId = await _mediator.Send(new SubmitFirmAdminRequestFromAccountCommand());
        return Ok(ApiResponse<int>.SuccessResponse(requestId, "Your request has been submitted. A Super Admin will review it and you'll be notified by email."));
    }








    [HttpGet("mine")]
    [Authorize]
    public async Task<IActionResult> GetMine()
    {
        var result = await _mediator.Send(new GetMyFirmAdminRequestQuery());
        return Ok(ApiResponse<FirmAdminRequestDTO?>.SuccessResponse(result, "Your latest request"));
    }







    [HttpPut("{id}/cancel")]
    [Authorize]
    public async Task<IActionResult> Cancel(int id)
    {
        var result = await _mediator.Send(new CancelFirmAdminRequestFromAccountCommand(id));
        return Ok(ApiResponse<bool>.SuccessResponse(result, "Request cancelled. You can now request a Firm Admin or send a new request."));
    }






    [HttpGet]
    [Authorize(Roles = RoleNames.SuperAdminOnly)]
    public async Task<IActionResult> GetAll([FromQuery] string? status)
    {
        var requests = await _mediator.Send(new GetFirmAdminRequestsQuery(status));
        return Ok(ApiResponse<List<FirmAdminRequestDTO>>.SuccessResponse(requests, "Firm Admin requests fetched"));
    }






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







    private int? GetActingUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(claim, out var id) ? id : null;
    }
}

public record RejectFirmAdminRequestBody(string? Reason);