using LTSBackend.Data;
using LTSBackend.Features.Dashboard.DTOs;
using LTSBackend.Services.CurrentUser;
using LTSBackend.Services.Permissions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.Dashboard.Queries.GetFirmDashboard;

public class GetFirmDashboardHandler(AppDbContext _context,ICurrentUserService _currentUser,IPermissionService _permissionService,
    ILogger<GetFirmDashboardHandler> _logger) : IRequestHandler<GetFirmDashboardQuery, FirmDashboardDTO>
{
    // =====================================================
    // HANDLE — firm-scoped statistics for every non-SuperAdmin role
    // Applies the same visibility rule as GetAllCasesHandler: FirmAdmin
    // and Partner see stats across the whole firm's case directory;
    // AssociateLawyer/Moharrir/InternParalegal only see stats for cases
    // they're actively assigned to — the dashboard must never imply
    // visibility a role doesn't actually have.
    // =====================================================
    public async Task<FirmDashboardDTO> Handle(GetFirmDashboardQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserID;
        if (userId == null)
        {
            _logger.LogWarning("Firm dashboard requested with no resolvable UserID");
            return new FirmDashboardDTO();
        }

        // Same rule GetAllCasesHandler applies to case listing: FirmAdmin
        // and Partner see the whole firm's case directory, everyone else
        // (AssociateLawyer/Moharrir/InternParalegal) only sees cases they
        // are actively assigned to. The dashboard must never imply
        // visibility a role doesn't actually have.
        bool hasFullVisibility = await _permissionService.HasFullCaseDirectoryVisibilityAsync(userId.Value, cancellationToken);
        var now = DateTime.UtcNow;

        // Cases and Deadlines/Hearings query filters already scope to the
        // caller's own firm (AppDbContext global query filters), so no
        // explicit FirmID check is needed here.
        var casesQuery = _context.Cases.AsNoTracking().Where(c => !c.IsArchived);

        if (!hasFullVisibility)
        {
            casesQuery = casesQuery.Where(c => _context.CaseAssignments.Any(a =>
                a.CaseID == c.CaseID &&
                a.UserID == userId.Value &&
                (a.EndDate == null || a.EndDate > now)));
        }

        var totalCases = await casesQuery.CountAsync(cancellationToken);
        var closedCases = await casesQuery.CountAsync(c => c.IsClosed, cancellationToken);
        var activeCases = totalCases - closedCases;

        var caseIdsQuery = casesQuery.Select(c => c.CaseID);
        var weekFromNow = now.AddDays(7);

        var upcomingHearings = await _context.Hearings
            .AsNoTracking()
            .CountAsync(h => caseIdsQuery.Contains(h.CaseID) && h.HearingDate >= now && h.HearingDate <= weekFromNow, cancellationToken);

        var pendingDeadlines = await _context.Deadlines
            .AsNoTracking()
            .CountAsync(d => caseIdsQuery.Contains(d.CaseID) && !d.Completed && d.DueDate >= now, cancellationToken);

        var overdueDeadlines = await _context.Deadlines
            .AsNoTracking()
            .CountAsync(d => caseIdsQuery.Contains(d.CaseID) && !d.Completed && d.DueDate < now, cancellationToken);

        var dto = new FirmDashboardDTO
        {
            Scope = hasFullVisibility ? "FirmWide" : "AssignedOnly",
            TotalCases = totalCases,
            ActiveCases = activeCases,
            ClosedCases = closedCases,
            UpcomingHearings7Days = upcomingHearings,
            PendingDeadlines = pendingDeadlines,
            OverdueDeadlines = overdueDeadlines,
            TotalFirmUsers = hasFullVisibility
                ? await _context.Users.CountAsync(u => u.IsActive && !u.IsDeleted, cancellationToken)
                : null
        };

        _logger.LogInformation("Firm dashboard fetched for user {UserId} - Scope: {Scope}, TotalCases: {TotalCases}",userId, dto.Scope, dto.TotalCases);

        return dto;
    }
}
