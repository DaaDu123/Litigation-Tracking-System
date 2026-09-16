using LTSBackend.Comman.Responses;
using LTSBackend.Features.AuditLogs.DTOs;
using LTSBackend.Features.AuditLogs.Queries.GetAuditLogs;
using LTSBackend.Features.Authorization;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LTSBackend.Features.AuditLogs.Controllers;

[Route("api/[controller]")]
[ApiController]
[HasPermission("ViewAuditLogs")]
public class AuditLogsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AuditLogsController> _logger;

    public AuditLogsController(IMediator mediator, ILogger<AuditLogsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    // =====================================================
    // GET ALL AUDIT LOGS — requires "ViewAuditLogs" permission
    // Returns a paged, filterable list of audit-log entries (search text,
    // action type, date range) so admins/auditors can review who did what
    // and when. Pagination keeps the response size bounded for firms with
    // a long audit history.
    // =====================================================
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] string? action, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        _logger.LogInformation("Get audit logs request - Page: {PageNumber}, Size: {PageSize}", pageNumber, pageSize);

        var result = await _mediator.Send(new GetAuditLogsQuery(search, fromDate, toDate, action, pageNumber, pageSize));
        return Ok(ApiResponse<PagedResult<AuditLogDTO>>.SuccessResponse(result, "Audit logs fetched successfully."));
    }
}