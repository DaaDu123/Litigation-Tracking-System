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

/// <summary>
/// Super Admin only - provisioning, blocking, removing and exporting
/// firm (law firm) workspaces. This is the multi-tenancy management
/// surface described in the Roles SRS §1 / §4.I.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = RoleNames.SuperAdminOnly)]
public class FirmsController(IMediator _mediator, ILogger<FirmsController> _logger) : ControllerBase
{
    // =====================================================
    // CREATE FIRM — SuperAdmin only
    // Provisions a brand-new, isolated firm workspace (tenant). This is
    // the platform-level entry point that lets a new law firm start using
    // LTS.
    // =====================================================
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

    // =====================================================
    // GET ALL FIRMS — SuperAdmin only
    // Lists every firm workspace on the platform, for the SuperAdmin
    // console.
    // =====================================================
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var firms = await _mediator.Send(new GetAllFirmsQuery());
        return Ok(ApiResponse<List<FirmDTO>>.SuccessResponse(firms, "Firms successfully fetched"));
    }

    // =====================================================
    // GET FIRM BY ID — SuperAdmin only
    // Fetches a single firm workspace's details.
    // =====================================================
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var firm = await _mediator.Send(new GetFirmByIdQuery(id));
        if (firm == null)
            return NotFound(ApiResponse<FirmDTO>.FailureResponse("Firm not found"));

        return Ok(ApiResponse<FirmDTO>.SuccessResponse(firm, "Firm successfully fetched"));
    }

    // =====================================================
    // UPDATE FIRM — SuperAdmin only
    // Edits a firm workspace's own details (name, contact info, etc.).
    // Route id and body FirmID must match.
    // =====================================================
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateFirmCommand command)
    {
        if (id != command.FirmID)
            return BadRequest(ApiResponse<bool>.FailureResponse("URL and body Firm ID do not match"));

        var result = await _mediator.Send(command);
        return Ok(ApiResponse<bool>.SuccessResponse(result, "Firm successfully updated"));
    }

    // =====================================================
    // BLOCK FIRM — SuperAdmin only
    // Suspends an entire firm's workspace, immediately preventing every
    // one of that firm's users from logging in (existing sessions/tokens
    // are also rejected going forward), without deleting any data.
    // =====================================================
    [HttpPut("{id}/block")]
    public async Task<IActionResult> Block(int id, [FromBody] BlockFirmRequest? body)
    {
        var actingUserId = GetActingUserId();
        if (actingUserId == null)
            return Unauthorized(ApiResponse<bool>.FailureResponse("Invalid identity."));

        var result = await _mediator.Send(new BlockFirmCommand(id, body?.Reason) { ActingUserID = actingUserId.Value });
        return Ok(ApiResponse<bool>.SuccessResponse(result, "Firm blocked - all users will be unable to log in."));
    }

    // =====================================================
    // UNBLOCK FIRM — SuperAdmin only
    // Reverses Block: restores the firm's ability to log in and use the
    // system.
    // =====================================================
    [HttpPut("{id}/unblock")]
    public async Task<IActionResult> Unblock(int id)
    {
        var result = await _mediator.Send(new UnblockFirmCommand(id));
        return Ok(ApiResponse<bool>.SuccessResponse(result, "Firm unblocked"));
    }

    // =====================================================
    // DELETE FIRM — SuperAdmin only
    // Permanently removes a firm workspace. Intended to be used after the
    // firm's data has already been exported (see Export below), per the
    // firm-offboarding flow in the SRS.
    // =====================================================
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var actingUserId = GetActingUserId();
        if (actingUserId == null)
            return Unauthorized(ApiResponse<bool>.FailureResponse("Invalid identity."));

        var result = await _mediator.Send(new DeleteFirmCommand(id) { ActingUserID = actingUserId.Value });
        return Ok(ApiResponse<bool>.SuccessResponse(result, "Firm removed"));
    }

    // =====================================================
    // EXPORT FIRM DATA — SuperAdmin only
    // Downloads a zip of the firm's users/cases/parties as CSV, so the
    // firm's data can be handed back to them before their workspace is
    // blocked/removed.
    // =====================================================
    [HttpGet("{id}/export")]
    public async Task<IActionResult> Export(int id)
    {
        var bytes = await _mediator.Send(new ExportFirmDataQuery(id));
        _logger.LogInformation("Firm {FirmID} data exported by {ActingUserID}", id, GetActingUserId());
        return File(bytes, "application/zip", $"firm-{id}-export-{DateTime.UtcNow:yyyyMMdd}.zip");
    }

    // =====================================================
    // GET ACTING USER ID — internal helper, not an endpoint
    // Reads the caller's own UserID from their JWT claim so
    // Create/Block/Delete/Export can record who acted, without ever
    // trusting a user ID from the request body.
    // =====================================================
    private int? GetActingUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(claim, out var id) ? id : null;
    }
}

public record BlockFirmRequest(string? Reason);
