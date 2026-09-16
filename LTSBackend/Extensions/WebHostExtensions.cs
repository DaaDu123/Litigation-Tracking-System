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
                        PermitLimit = 100,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0
                    }));

            // Tighter limits for auth endpoints - "critical" covers anything
            // that lets an attacker guess a secret (password, OTP, refresh
            // token); "moderate" covers spam/enumeration-prone endpoints
            // (register, forgot-password, resend-otp).
            options.AddPolicy("auth-critical", httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: _ => new FixedWindowRateLimiterOptions { PermitLimit = 5, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));

            options.AddPolicy("auth-moderate", httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: _ => new FixedWindowRateLimiterOptions { PermitLimit = 10, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
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
        builder.Services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            options.KnownIPNetworks.Clear();
            options.KnownProxies.Clear();
        });

        return builder;
    }
}
