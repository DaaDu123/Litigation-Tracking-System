using LTSBackend.Data;
using LTSBackend.Models.Audit;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Services.Audit;

public class AuditLogService(AppDbContext _context, ILogger<AuditLogService> _logger) : IAuditLogService
{
    public async Task LogAsync(AuditLog log)
    {
        try
        {
            _context.AuditLogs.Add(log);
            await _context.SaveChangesAsync();
            _logger.LogDebug("Audit log saved: {Action} by User {UserId}", log.Action, log.UserID);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save audit log for action: {Action}", log.Action);
            throw;
        }
    }

    public async Task<List<AuditLog>> GetAllAsync()
    {
        return await _context.AuditLogs
            .AsNoTracking()
            .OrderByDescending(x => x.Timestamp)
            .ToListAsync();
    }
}