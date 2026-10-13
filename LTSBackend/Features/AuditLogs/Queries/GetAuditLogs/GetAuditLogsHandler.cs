using LTSBackend.Comman.Responses;
using LTSBackend.Data;
using LTSBackend.Features.AuditLogs.DTOs;
using LTSBackend.Services.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.AuditLogs.Queries.GetAuditLogs;

public class GetAuditLogsHandler (AppDbContext _context, ICurrentUserService _currentUser, ILogger<GetAuditLogsHandler> _logger) : IRequestHandler<GetAuditLogsQuery, PagedResult<AuditLogDTO>>
{











    public async Task<PagedResult<AuditLogDTO>> Handle(GetAuditLogsQuery request,CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching audit logs - Page: {PageNumber}, Size: {PageSize}",request.PageNumber,request.PageSize);

        var query = _context.AuditLogs
            .AsNoTracking()
            .Include(x => x.User)
            .AsQueryable();








        if (!_currentUser.IsSuperAdmin)
            query = query.Where(x => x.User != null && x.User.FirmID == _currentUser.FirmID);


        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();

            query = query.Where(x => x.User != null && (x.User.FullName.Contains(search) || x.User.Email.Contains(search)));

            _logger.LogInformation("Applied search filter: {Search}", search);
        }


        if (!string.IsNullOrWhiteSpace(request.Action))
        {
            query = query.Where(x =>x.Action == request.Action.Trim());

            _logger.LogInformation("Applied action filter: {Action}", request.Action);
        }


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


        var total = await query.CountAsync(cancellationToken);


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