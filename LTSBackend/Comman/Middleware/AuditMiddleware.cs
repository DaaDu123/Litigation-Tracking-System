using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace LTSBackend.Comman.Middleware;

public class AuditMiddleware(RequestDelegate next, ILogger<AuditMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var ip = context.Connection.RemoteIpAddress?.ToString();
        logger.LogInformation(
            "Incoming {Method} {Path} from {IP}",
            context.Request.Method,
            context.Request.Path,
            ip);

        await next(context);
    }
}