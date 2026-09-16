using LTSBackend.Services.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;

namespace LTSBackend.Features.Authorization;

public class PermissionHandler(IServiceScopeFactory _scopeFactory, ILogger<PermissionHandler> _logger) : AuthorizationHandler<PermissionRequirement>
{
    // =====================================================
    // HANDLE REQUIREMENT ASYNC — backs the [HasPermission("...")] attribute
    // Reads the caller's UserID from their JWT claim and asks
    // IPermissionService whether they hold the required permission. A
    // fresh DI scope (and therefore a fresh DbContext) is created per
    // check since AuthorizationHandler instances are singleton-scoped.
    // Never throws for a missing/invalid claim — simply fails the
    // requirement (no context.Succeed call), which ASP.NET Core turns
    // into a 403.
    // =====================================================
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
        {
            _logger.LogWarning("Invalid or missing user identity claim");
            return;
        }
        // Create a new scope for each request — ensures fresh DbContext
        using var scope = _scopeFactory.CreateScope();
        var permissionService = scope.ServiceProvider.GetRequiredService<IPermissionService>();

        bool hasPermission = await permissionService.HasPermissionAsync(userId, requirement.Permission);
        if (hasPermission)
        {
            _logger.LogDebug("User {UserId} authorized for permission {Permission}", userId, requirement.Permission);
            context.Succeed(requirement);
        }
        else
        {
            _logger.LogWarning("User {UserId} denied permission {Permission}", userId, requirement.Permission);
        }
    }
}