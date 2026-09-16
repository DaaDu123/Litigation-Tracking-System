using LTSFrontend.Core.DTOs;
using LTSFrontend.Features.AuditLogs.DTOs;

namespace LTSFrontend.Features.AuditLogs.Services
{
    public interface IAuditLogService
    {
        Task<PagedResult<AuditLogDTO>> GetAllAsync(AuditLogFilterDTO filter);
    }
}
