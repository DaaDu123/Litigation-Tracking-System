using LTSBackend.Data;
using LTSBackend.Services.Email;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Services.BackgroundServices;
public class NotificationEmailDispatcherService(IServiceScopeFactory scopeFactory,ILogger<NotificationEmailDispatcherService> logger) : BackgroundService
{
    private static readonly TimeSpan CheckInterval = TimeSpan.FromMinutes(2);
    private const int BatchSize = 50;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await DispatchPendingNotificationEmailsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error dispatching notification emails");
            }

            await Task.Delay(CheckInterval, stoppingToken);
        }
    }

    private async Task DispatchPendingNotificationEmailsAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

        var pending = await context.Notifications
            .Where(n => !n.IsSent)
            .OrderBy(n => n.CreatedDate)
            .Take(BatchSize)
            .Include(n => n.User)
            .ToListAsync(ct);

        if (pending.Count == 0)
            return;

        int sent = 0, skipped = 0;

        foreach (var notification in pending)
        {
            if (notification.User == null || string.IsNullOrWhiteSpace(notification.User.Email) || !notification.User.IsActive || notification.User.IsDeleted)
            {
                notification.IsSent = true;
                notification.SentDate = DateTime.UtcNow;
                skipped++;
                continue;
            }

            try
            {
                await emailService.SendNotificationEmailAsync(
                    notification.User.Email,
                    notification.User.FullName,
                    notification.Subject ?? "New notification",
                    notification.Message ?? string.Empty);

                notification.IsSent = true;
                notification.SentDate = DateTime.UtcNow;
                sent++;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send notification email {NotificationId} to {Email}",notification.NotificationID, notification.User.Email);
            }
        }

        await context.SaveChangesAsync(ct);
        logger.LogInformation("Notification email dispatch: {Sent} sent, {Skipped} skipped (inactive/no email)", sent, skipped);
    }
}
