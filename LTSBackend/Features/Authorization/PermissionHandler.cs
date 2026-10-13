using LTSBackend.Services.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;

namespace LTSBackend.Features.Authorization;

public class PermissionHandler(IServiceScopeFactory _scopeFactory, ILogger<PermissionHandler> _logger) : AuthorizationHandler<PermissionRequirement>
{










    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
        {
            _logger.LogWarning("Invalid or missing user identity claim");
            return;
        }

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