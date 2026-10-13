using LTSBackend.Data;
using LTSBackend.Models.Cases;
using LTSBackend.Models.Security;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Services.BackgroundServices;

public class ReminderService(IServiceScopeFactory scopeFactory, ILogger<ReminderService> logger) : BackgroundService
{



    private static readonly TimeSpan ScheduledRunTimeUtc = TimeSpan.FromHours(8);

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