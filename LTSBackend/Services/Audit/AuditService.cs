using LTSBackend.Models.Audit;
namespace LTSBackend.Services.Audit;

public class AuditService(IHttpContextAccessor _httpContextAccessor) : IAuditService
{
    public AuditLog Create(int? userId, string action)
    {
        var ip = _httpContextAccessor.HttpContext?
            .Connection
            .RemoteIpAddress?
            .ToString();

        return new AuditLog
        {
            UserID = userId,
            Action = action,
            IPAddress = ip,
            Timestamp = DateTime.UtcNow
        };
    }
}