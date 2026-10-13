namespace LTSBackend.Extensions;

public static class LoggingExtensions
{
    public static WebApplicationBuilder AddAppLogging(this WebApplicationBuilder builder)
    {
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();
        builder.Logging.AddDebug();


        if (!builder.Environment.IsDevelopment() && OperatingSystem.IsWindows())
        {
            builder.Logging.AddEventLog();
        }

        return builder;
    }
}
