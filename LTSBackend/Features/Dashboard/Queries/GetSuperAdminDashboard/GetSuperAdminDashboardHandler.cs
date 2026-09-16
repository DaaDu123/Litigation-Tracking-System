using LTSBackend.Comman.Enum;
using LTSBackend.Data;
using LTSBackend.Features.Dashboard.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.Dashboard.Queries.GetSuperAdminDashboard;

public class GetSuperAdminDashboardHandler(AppDbContext _context, ILogger<GetSuperAdminDashboardHandler> _logger) : IRequestHandler<GetSuperAdminDashboardQuery, SuperAdminDashboardDTO>
{
    // =====================================================
    // HANDLE — platform-wide statistics for the SuperAdmin dashboard
    // Returns firm counts, audit activity, and other platform-owner-level
    // metrics — never case/hearing/document data, which is firm-internal
    // business outside the SuperAdmin's scope.
    // =====================================================
    public async Task<SuperAdminDashboardDTO> Handle(GetSuperAdminDashboardQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching SuperAdmin dashboard statistics");

        var dto = new SuperAdminDashboardDTO
        {
            TotalFirms = await _context.Firms.CountAsync(cancellationToken),
            ActiveFirms = await _context.Firms.CountAsync(f => !f.IsBlocked, cancellationToken),
            BlockedFirms = await _context.Firms.CountAsync(f => f.IsBlocked, cancellationToken),
            PendingDomainMigrations = await _context.Firms.CountAsync(f => f.MigrationStatus == "Requested" || f.MigrationStatus == "InProgress", cancellationToken),

            TotalFirmAdmins = await _context.Users.CountAsync(u => u.IsActive && !u.IsDeleted && u.RoleID == (int)UserRole.FirmAdmin, cancellationToken),

            TotalSystemAuditLogs = await _context.AuditLogs.CountAsync(cancellationToken),
            TotalActiveSessions = await _context.RefreshTokens.CountAsync(t => !t.IsRevoked && t.ExpiryDate > DateTime.UtcNow, cancellationToken),

            RecentActivities = await _context.AuditLogs
                .AsNoTracking()
                .OrderByDescending(x => x.Timestamp)
                .Take(10)
                .Select(x => new RecentActivityDTO
                {
                    LogID = x.LogID,
                    UserID = x.UserID,
                    Action = x.Action,
                    Timestamp = x.Timestamp
                })
                .ToListAsync(cancellationToken)
        };

        _logger.LogInformation("SuperAdmin dashboard fetched: {TotalFirms} firms, {ActiveFirms} active, {TotalFirmAdmins} firm admins",dto.TotalFirms, dto.ActiveFirms, dto.TotalFirmAdmins);

        return dto;
    }
}
