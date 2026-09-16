namespace LTSBackend.Extensions;

public static class LoggingExtensions
{
    public static WebApplicationBuilder AddAppLogging(this WebApplicationBuilder builder)
    {
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();
        builder.Logging.AddDebug();

        // Windows Event Log only makes sense outside local dev, and only on Windows.
        if (!builder.Environment.IsDevelopment() && OperatingSystem.IsWindows())
        {
            builder.Logging.AddEventLog();
        }

        return builder;
    }
}
