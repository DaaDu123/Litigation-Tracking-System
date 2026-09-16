using LTSBackend.Data;
using LTSBackend.Models.Cases;
using LTSBackend.Models.Security;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Services.BackgroundServices;

public class ReminderService(IServiceScopeFactory scopeFactory, ILogger<ReminderService> logger) : BackgroundService
{
    // The sweep now runs once a day at a fixed wall-clock time (UTC) instead
    // of every few hours, so reminder emails go out at a predictable time
    // every morning rather than drifting depending on when the app started.
    private static readonly TimeSpan ScheduledRunTimeUtc = TimeSpan.FromHours(8); // 8:00 AM UTC

    private const string DeadlineAlertType = "DeadlineAlert";
    private const string HearingReminderType = "HearingReminder";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var delay = GetDelayUntilNextRun(DateTime.UtcNow, ScheduledRunTimeUtc);

            try
            {
                await Task.Delay(delay, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            if (stoppingToken.IsCancellationRequested)
                break;

            try
            {
                await GenerateRemindersAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error generating reminders");
            }
        }
    }

    /// <summary>
    /// Calculates how long to wait before the next scheduled sweep so that it
    /// always lands at <paramref name="scheduledTimeUtc"/> (UTC) — today if
    /// that time hasn't passed yet, otherwise tomorrow at the same time.
    /// Kept as an internal static method so it can be unit tested without
    /// spinning up the hosted service.
    /// </summary>
    internal static TimeSpan GetDelayUntilNextRun(DateTime nowUtc, TimeSpan scheduledTimeUtc)
    {
        var todayRun = nowUtc.Date.Add(scheduledTimeUtc);
        var nextRun = nowUtc <= todayRun ? todayRun : todayRun.AddDays(1);
        return nextRun - nowUtc;
    }

    private async Task GenerateRemindersAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var today = DateTime.UtcNow.Date;

        // Resolve NotificationType IDs (seeded rows)
        var deadlineTypeId = await context.NotificationTypes
            .Where(t => t.TypeName == DeadlineAlertType)
            .Select(t => (int?)t.NotificationTypeID)
            .FirstOrDefaultAsync(ct);

        var hearingTypeId = await context.NotificationTypes
            .Where(t => t.TypeName == HearingReminderType)
            .Select(t => (int?)t.NotificationTypeID)
            .FirstOrDefaultAsync(ct);

        if (deadlineTypeId == null || hearingTypeId == null)
        {
            logger.LogError("NotificationType seed rows missing ({DeadlineType}/{HearingType}) — skipping reminder sweep",DeadlineAlertType, HearingReminderType);
            return;
        }

        // DEADLINE REMINDERS
        // Each deadline lets the user pick how many days beforehand they want
        // to be warned (Deadline.ReminderDays, set when the deadline is
        // created/updated). That custom window is respected here — but as a
        // safety net, a reminder is ALWAYS guaranteed once 7 days (one week)
        // or less remain, even if the user configured a shorter custom
        // window (e.g. 2 days). So a deadline qualifies if EITHER:
        //   a) today falls inside the user's own ReminderDays window, or
        //   b) one week or less remains until the due date (guaranteed floor)
        var dueDeadlines = await context.Deadlines
            .Where(d => !d.Completed &&
                        d.DueDate >= today &&
                        (d.DueDate.AddDays(-d.ReminderDays) <= today || d.DueDate.AddDays(-7) <= today))
            .ToListAsync(ct);

        foreach (var deadline in dueDeadlines)
        {
            var assignedUserIds = await context.CaseAssignments
                .Where(a => a.CaseID == deadline.CaseID && (a.EndDate == null || a.EndDate > DateTime.UtcNow))
                .Select(a => a.UserID)
                .ToListAsync(ct);

            foreach (var userId in assignedUserIds)
            {
                bool alreadyNotified = await context.Notifications.AnyAsync(n =>
                    n.NotificationTypeID == deadlineTypeId &&
                    n.CaseID == deadline.CaseID &&
                    n.UserID == userId &&
                    n.CreatedDate.Date == today, ct);

                if (alreadyNotified) continue;

                context.Notifications.Add(new Notification
                {
                    NotificationTypeID = deadlineTypeId.Value,
                    UserID = userId,
                    CaseID = deadline.CaseID,
                    Subject = "Deadline approaching",
                    Message = $"Deadline '{deadline.DeadlineType}' is due on {deadline.DueDate:yyyy-MM-dd}.",
                    Priority = "High",
                    IsRead = false,
                    IsSent = false,
                    CreatedDate = DateTime.UtcNow
                });
            }
        }

        // HEARING REMINDERS (next 2 days)
        var upcomingHearings = await context.Hearings.Where(h => h.HearingDate.Date >= today && h.HearingDate.Date <= today.AddDays(2)).ToListAsync(ct);

        foreach (var hearing in upcomingHearings)
        {
            var assignedUserIds = await context.CaseAssignments
                .Where(a => a.CaseID == hearing.CaseID && (a.EndDate == null || a.EndDate > DateTime.UtcNow))
                .Select(a => a.UserID)
                .ToListAsync(ct);

            foreach (var userId in assignedUserIds)
            {
                bool alreadyNotified = await context.Notifications.AnyAsync(n =>
                    n.NotificationTypeID == hearingTypeId &&
                    n.CaseID == hearing.CaseID &&
                    n.UserID == userId &&
                    n.CreatedDate.Date == today, ct);

                if (alreadyNotified) continue;

                context.Notifications.Add(new Notification
                {
                    NotificationTypeID = hearingTypeId.Value,
                    UserID = userId,
                    CaseID = hearing.CaseID,
                    Subject = "Upcoming hearing",
                    Message = $"Hearing scheduled on {hearing.HearingDate:yyyy-MM-dd}.",
                    Priority = "Critical",
                    IsRead = false,
                    IsSent = false,
                    CreatedDate = DateTime.UtcNow
                });
            }
        }

        await context.SaveChangesAsync(ct);
        logger.LogInformation("Reminder sweep complete: {Deadlines} deadlines, {Hearings} hearings checked",dueDeadlines.Count, upcomingHearings.Count);
    }
}