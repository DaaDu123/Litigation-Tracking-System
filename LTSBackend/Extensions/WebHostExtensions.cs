using LTSBackend.Data;
using LTSBackend.Features.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using System.Threading.RateLimiting;

namespace LTSBackend.Extensions;

public static class WebHostExtensions
{
    public static WebApplicationBuilder AddAppCors(this WebApplicationBuilder builder)
    {
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
                policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());

            if (!builder.Environment.IsDevelopment())
            {
                options.AddPolicy("Production", policy =>
                    policy.WithOrigins(builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? [])
                        .AllowAnyHeader()
                        .AllowAnyMethod());
            }
        });

        return builder;
    }

    // One ASP.NET Core [Authorize(Policy = "X")] per permission name, each
    // backed by PermissionRequirement/PermissionHandler - see Features/Authorization.
    public static IServiceCollection AddAppAuthorizationPolicies(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            var permissions = new[]
            {
                "ViewUsers", "CreateUsers", "UpdateUsers", "DeleteUsers",
                "ManageRoles", "ViewAuditLogs", "ViewDashboard",
                "ViewLoginHistory",
                "UploadDocuments", "ViewDocuments", "DownloadDocuments", "DeleteDocuments"
            };

            foreach (var permission in permissions)
            {
                options.AddPolicy(permission, policy => policy.Requirements.Add(new PermissionRequirement(permission)));
            }
        });

        return services;
    }

    public static IServiceCollection AddAppRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        // The Blazor Server frontend calls this API from ONE server IP for
                        // every user, so this per-IP bucket is shared by all users at once.
                        PermitLimit = 600,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0
                    }));

            // Tighter limits for auth endpoints - "critical" covers anything
            // that lets an attacker guess a secret (password, OTP, refresh
            // token); "moderate" covers spam/enumeration-prone endpoints
            // (register, forgot-password, resend-otp).
            options.AddPolicy("auth-critical", httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: _ => new FixedWindowRateLimiterOptions { PermitLimit = 30, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));

            // Silent access-token refresh. Not a guessable secret (long random cookie), and the frontend
            // fires it automatically for all users from the same IP, so it gets its own roomy bucket
            // instead of sharing "auth-critical" with login/OTP/reset-password.
            options.AddPolicy("auth-refresh", httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: _ => new FixedWindowRateLimiterOptions { PermitLimit = 300, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));

            options.AddPolicy("auth-moderate", httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: _ => new FixedWindowRateLimiterOptions { PermitLimit = 30, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
        });

        return services;
    }

    public static IServiceCollection AddAppHealthChecks(this IServiceCollection services)
    {
        services.AddHealthChecks().AddDbContextCheck<AppDbContext>();
        return services;
    }

    public static WebApplicationBuilder AddAppForwardedHeaders(this WebApplicationBuilder builder)
    {
        // The Blazor frontend forwards each user's real IP in X-Forwarded-For so the rate
        // limiters (partitioned by RemoteIpAddress) count PER USER instead of lumping everyone
        // behind the frontend server's single IP.
        //
        // SECURITY: set "ForwardedHeaders:KnownProxies" (appsettings) to the frontend server's
        // IP(s) in production. Only those hosts are then allowed to supply X-Forwarded-For, so
        // nobody calling the API directly can spoof an IP to dodge the limits. Left empty it
        // trusts any caller (the previous behaviour) so nothing breaks until you set it.
        var knownProxies = builder.Configuration.GetSection("ForwardedHeaders:KnownProxies").Get<string[]>() ?? [];

        builder.Services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            options.KnownIPNetworks.Clear();
            options.KnownProxies.Clear();

            foreach (var proxy in knownProxies)
            {
                if (System.Net.IPAddress.TryParse(proxy, out var address))
                    options.KnownProxies.Add(address);
            }
        });

        return builder;
    }
}
