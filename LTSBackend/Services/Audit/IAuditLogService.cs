using LTSBackend.Models.Audit;

namespace LTSBackend.Services.Audit;

public interface IAuditLogService
{
    Task LogAsync(AuditLog log);

    Task<List<AuditLog>> GetAllAsync();
}