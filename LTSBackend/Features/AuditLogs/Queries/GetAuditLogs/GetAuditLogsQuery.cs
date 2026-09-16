using LTSBackend.Comman.Responses;
using LTSBackend.Features.AuditLogs.DTOs;
using MediatR;

namespace LTSBackend.Features.AuditLogs.Queries.GetAuditLogs;

public record GetAuditLogsQuery(
    string? Search,
    DateTime? FromDate,
    DateTime? ToDate,
    string? Action,
    int PageNumber = 1,
    int PageSize = 10
) : IRequest<PagedResult<AuditLogDTO>>;
