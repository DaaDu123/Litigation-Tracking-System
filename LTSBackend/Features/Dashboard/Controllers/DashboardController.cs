using LTSBackend.Comman.Responses;
using LTSBackend.Features.Authorization;
using LTSBackend.Features.Dashboard.Queries.GetFirmDashboard;
using LTSBackend.Features.Dashboard.Queries.GetSuperAdminDashboard;
using LTSBackend.Services.CurrentUser;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LTSBackend.Features.Dashboard.Controllers;

/// <summary>
/// Every role gets its OWN dashboard, routed strictly off the caller's own
/// JWT role claim - nobody can request another role's dashboard shape
/// (Roles SRS: "no one can use or view another role's dashboard").
/// SuperAdmin gets a platform-wide, firm/audit-only view; every other role
/// gets a firm-scoped case/hearing/deadline view (further narrowed to
/// "assigned only" for AssociateLawyer/Moharrir/InternParalegal).
/// </summary>
[Route("api/[controller]")]
[ApiController]
[HasPermission("ViewDashboard")]
public class DashboardController(IMediator _mediator, ICurrentUserService _currentUser, ILogger<DashboardController> _logger) : ControllerBase
{
    // =====================================================
    // GET DASHBOARD STATS — requires "ViewDashboard" permission
    // Returns the statistics for whichever dashboard matches the caller's
    // OWN role — never a role picked by the request. SuperAdmin gets the
    // platform-wide (firm/audit) view; every other role gets the firm's
    // case/hearing/deadline view (further narrowed to "assigned only" for
    // AssociateLawyer/Moharrir/InternParalegal inside GetFirmDashboardQuery).
    // =====================================================
    [HttpGet]
    public async Task<IActionResult> GetStats()
    {
        if (_currentUser.IsSuperAdmin)
        {
            _logger.LogInformation("Get SuperAdmin dashboard request");
            var superAdminResult = await _mediator.Send(new GetSuperAdminDashboardQuery());
            return Ok(ApiResponse<object>.SuccessResponse(superAdminResult, "Dashboard statistics fetched successfully."));
        }

        _logger.LogInformation("Get firm dashboard request");
        var firmResult = await _mediator.Send(new GetFirmDashboardQuery());
        return Ok(ApiResponse<object>.SuccessResponse(firmResult, "Dashboard statistics fetched successfully."));
    }
}
