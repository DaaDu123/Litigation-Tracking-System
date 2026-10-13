using LTSBackend.Comman.Responses;
using LTSBackend.Features.Firms.Commands.BlockFirm;
using LTSBackend.Features.Firms.Commands.CreateFirm;
using LTSBackend.Features.Firms.Commands.DeleteFirm;
using LTSBackend.Features.Firms.Commands.UnblockFirm;
using LTSBackend.Features.Firms.Commands.UpdateFirm;
using LTSBackend.Features.Firms.DTOs;
using LTSBackend.Features.Firms.Queries.ExportFirmData;
using LTSBackend.Features.Firms.Queries.GetAllFirms;
using LTSBackend.Features.Firms.Queries.GetFirmById;
using LTSBackend.Models.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LTSBackend.Features.Firms.Controllers;






[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = RoleNames.SuperAdminOnly)]
public class FirmsController(IMediator _mediator, ILogger<FirmsController> _logger) : ControllerBase
{






    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateFirmCommand command)
    {
        var actingUserId = GetActingUserId();
        if (actingUserId == null)
            return Unauthorized(ApiResponse<int>.FailureResponse("Invalid identity."));

        var id = await _mediator.Send(command with { ActingUserID = actingUserId.Value });

        return CreatedAtAction(nameof(GetById), new { id },
            ApiResponse<int>.SuccessResponse(id, "Firm workspace successfully created"));
    }






    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var firms = await _mediator.Send(new GetAllFirmsQuery());
        return Ok(ApiResponse<List<FirmDTO>>.SuccessResponse(firms, "Firms successfully fetched"));
    }





    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var firm = await _mediator.Send(new GetFirmByIdQuery(id));
        if (firm == null)
            return NotFound(ApiResponse<FirmDTO>.FailureResponse("Firm not found"));

        return Ok(ApiResponse<FirmDTO>.SuccessResponse(firm, "Firm successfully fetched"));
    }






    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateFirmCommand command)
    {
        if (id != command.FirmID)
            return BadRequest(ApiResponse<bool>.FailureResponse("URL and body Firm ID do not match"));

        var result = await _mediator.Send(command);
        return Ok(ApiResponse<bool>.SuccessResponse(result, "Firm successfully updated"));
    }







    [HttpPut("{id}/block")]
    public async Task<IActionResult> Block(int id, [FromBody] BlockFirmRequest? body)
    {
        var actingUserId = GetActingUserId();
        if (actingUserId == null)
            return Unauthorized(ApiResponse<bool>.FailureResponse("Invalid identity."));

        var result = await _mediator.Send(new BlockFirmCommand(id, body?.Reason) { ActingUserID = actingUserId.Value });
        return Ok(ApiResponse<bool>.SuccessResponse(result, "Firm blocked - all users will be unable to log in."));
    }






    [HttpPut("{id}/unblock")]
    public async Task<IActionResult> Unblock(int id)
    {
        var result = await _mediator.Send(new UnblockFirmCommand(id));
        return Ok(ApiResponse<bool>.SuccessResponse(result, "Firm unblocked"));
    }







    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var actingUserId = GetActingUserId();
        if (actingUserId == null)
            return Unauthorized(ApiResponse<bool>.FailureResponse("Invalid identity."));

        var result = await _mediator.Send(new DeleteFirmCommand(id) { ActingUserID = actingUserId.Value });
        return Ok(ApiResponse<bool>.SuccessResponse(result, "Firm removed"));
    }







    [HttpGet("{id}/export")]
    public async Task<IActionResult> Export(int id)
    {
        var bytes = await _mediator.Send(new ExportFirmDataQuery(id));
        _logger.LogInformation("Firm {FirmID} data exported by {ActingUserID}", id, GetActingUserId());
        return File(bytes, "application/zip", $"firm-{id}-export-{DateTime.UtcNow:yyyyMMdd}.zip");
    }







    private int? GetActingUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(claim, out var id) ? id : null;
    }
}

public record BlockFirmRequest(string? Reason);
