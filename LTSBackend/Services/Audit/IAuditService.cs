using LTSBackend.Models.Audit;
public interface IAuditService
{
    AuditLog Create(int? userId, string action);
}