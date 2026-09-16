using LTSBackend.Comman.Responses;
using LTSBackend.Data;
using LTSBackend.Features.AuditLogs.DTOs;
using LTSBackend.Services.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.AuditLogs.Queries.GetAuditLogs;

public class GetAuditLogsHandler (AppDbContext _context, ICurrentUserService _currentUser, ILogger<GetAuditLogsHandler> _logger) : IRequestHandler<GetAuditLogsQuery, PagedResult<AuditLogDTO>>
{
 

    // =====================================================
    // HANDLE — paged, filterable (search/action/date range) audit log list
    // Tenant-scoped explicitly here (AuditLog has no FirmID column of its
    // own, so scoping goes through the related User's FirmID) in addition
    // to relying on any global query filter, as defence in depth — a
    // FirmAdmin/Auditor must only ever see their own firm's audit trail;
    // SuperAdmin sees every firm's, matching the system-wide audit log
    // required by the SRS.
    // =====================================================
    public async Task<PagedResult<AuditLogDTO>> Handle(GetAuditLogsQuery request,CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching audit logs - Page: {PageNumber}, Size: {PageSize}",request.PageNumber,request.PageSize);

        var query = _context.AuditLogs
            .AsNoTracking()
            .Include(x => x.User)
            .AsQueryable();

        // CRITICAL FIX (cross-tenant data leak): AuditLog has no FirmID
        // column, and this query previously applied no tenant scoping at
        // all - relying entirely on AppDbContext's global query filter,
        // which itself did not exist for this entity until this same
        // review pass. Explicit scoping here as well (defense in depth,
        // matching this codebase's convention elsewhere) so this can never
        // regress even if a future change reads via IgnoreQueryFilters().
        if (!_currentUser.IsSuperAdmin)
            query = query.Where(x => x.User != null && x.User.FirmID == _currentUser.FirmID);

        // 1. Search by user name or email
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();

            query = query.Where(x => x.User != null && (x.User.FullName.Contains(search) || x.User.Email.Contains(search)));

            _logger.LogInformation("Applied search filter: {Search}", search);
        }

        // 2. Filter by action
        if (!string.IsNullOrWhiteSpace(request.Action))
        {
            query = query.Where(x =>x.Action == request.Action.Trim());

            _logger.LogInformation("Applied action filter: {Action}", request.Action);
        }

        // 3. Filter by date range
        if (request.FromDate.HasValue)
        {
            query = query.Where(x =>x.Timestamp >= request.FromDate.Value);

            _logger.LogInformation("Applied from date filter: {FromDate}", request.FromDate);
        }

        if (request.ToDate.HasValue)
        {
            query = query.Where(x =>x.Timestamp <= request.ToDate.Value);

            _logger.LogInformation("Applied to date filter: {ToDate}", request.ToDate);
        }

        // 4. Get total record count
        var total = await query.CountAsync(cancellationToken);

        // 5. Apply pagination and ordering
        var items = await query
            .OrderByDescending(x => x.Timestamp)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(x => new AuditLogDTO
            {
                LogID = x.LogID,
                UserID = x.UserID,
                UserName = x.User != null ? x.User.FullName : "System",
                Action = x.Action,
                IPAddress = x.IPAddress,
                Timestamp = x.Timestamp
            })
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Fetched {Count} audit logs of {Total} total",items.Count,total);

        return new PagedResult<AuditLogDTO>
        {
            Items = items,
            TotalRecords = total,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}