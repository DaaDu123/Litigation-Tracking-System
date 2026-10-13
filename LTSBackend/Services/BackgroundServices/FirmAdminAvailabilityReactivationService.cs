using LTSBackend.Data;
using LTSBackend.Models.Security;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Services.BackgroundServices;











public class FirmAdminAvailabilityReactivationService(IServiceScopeFactory scopeFactory, ILogger<FirmAdminAvailabilityReactivationService> logger) : BackgroundService
{
    private static readonly TimeSpan SweepInterval = TimeSpan.FromMinutes(5);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ReactivateExpiredAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error running Firm Admin availability reactivation sweep");
            }

            try
            {
                await Task.Delay(SweepInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private async Task ReactivateExpiredAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var now = DateTime.UtcNow;

        var expired = await context.Users
            .IgnoreQueryFilters()
            .Where(x => !x.IsAvailable && x.InactiveUntilUtc != null && x.InactiveUntilUtc <= now && !x.IsDeleted)
            .ToListAsync(ct);

        if (expired.Count == 0)
            return;

        foreach (var user in expired)
        {
            user.IsAvailable = true;
            user.InactiveReason = null;
            user.InactiveFromUtc = null;
            user.InactiveUntilUtc = null;

            if (user.FirmID.HasValue)
            {
                context.FirmMembershipEvents.Add(new FirmMembershipEvent
                {
                    FirmID = user.FirmID.Value,
                    UserID = user.UserID,
                    ActionType = "AutoReactivated",
                    PerformedByUserID = null,
                    PerformedAtUtc = now
                });
            }
        }

        await context.SaveChangesAsync(ct);
        logger.LogInformation("Availability reactivation sweep: {Count} user(s) auto-reactivated", expired.Count);
    }
}
