using LTSBackend.Features.Authorization;
using LTSBackend.Services;
using LTSBackend.Services.Audit;
using LTSBackend.Services.BackgroundServices;
using LTSBackend.Services.CurrentUser;
using LTSBackend.Services.DocumentPermissions;
using LTSBackend.Services.Email;
using LTSBackend.Services.Jwt;
using LTSBackend.Services.Permissions;
using LTSBackend.Services.ProfileService;
using LTSBackend.Services.VirusScan;
using Microsoft.AspNetCore.Authorization;

namespace LTSBackend.Extensions;

public static class ApplicationServicesExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<IJwtService, JwtService>();

        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IFileService, FileService>();
        services.AddScoped<IVirusScanService, ClamAvVirusScanService>();

        // Per-document permission overrides - drives the Moharrir "blind
        // upload" restriction (write-only until a Partner/FirmAdmin grants view/download).
        services.AddScoped<IDocumentPermissionService, DocumentPermissionService>();

        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddScoped<IPermissionService, PermissionService>();

        services.AddScoped<IAuthorizationHandler, PermissionHandler>();
        services.AddSingleton<DefaultAuthorizationPolicyProvider>();
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();

        return services;
    }

    public static IServiceCollection AddAppBackgroundServices(this IServiceCollection services)
    {
        services.AddHostedService<ReminderService>();
        services.AddHostedService<NotificationEmailDispatcherService>();
        services.AddHostedService<RefreshTokenCleanupService>();
        services.AddHostedService<FirmAdminAvailabilityReactivationService>();

        return services;
    }
}
