using LTSBackend.Data;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Services.BackgroundServices;

public class RefreshTokenCleanupService(IServiceScopeFactory scopeFactory,ILogger<RefreshTokenCleanupService> logger) : BackgroundService
{
    private static readonly TimeSpan CleanupInterval = TimeSpan.FromHours(1);
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("RefreshTokenCleanupService started. Will run every {Hours} hours.",CleanupInterval.TotalHours);
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CleanupExpiredTokensAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during refresh token cleanup");
            }

            await Task.Delay(CleanupInterval, stoppingToken);
        }
        logger.LogInformation("RefreshTokenCleanupService stopped.");
    }

    private async Task CleanupExpiredTokensAsync(CancellationToken stoppingToken)
    {
        using var scope = scopeFactory.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var expiredTokens = await context.RefreshTokens.Where(x => x.IsRevoked || x.ExpiryDate <= DateTime.UtcNow).ToListAsync(stoppingToken);
        if (expiredTokens.Count == 0)
            return;

        context.RefreshTokens.RemoveRange(expiredTokens);

        await context.SaveChangesAsync(stoppingToken);

        logger.LogInformation("Cleaned up {Count} expired/revoked refresh tokens",expiredTokens.Count);
    }
}