using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace LTSBackend.Features.Authorization;

/// <summary>
/// Dynamically resolves an AuthorizationPolicy for any policy name that isn't
/// explicitly registered via options.AddPolicy(...). Required because
/// [HasPermission("SomePermission")] sets AuthorizeAttribute.Policy to an
/// arbitrary permission string - without this provider, ASP.NET Core's
/// default policy resolution throws:
///   InvalidOperationException: The AuthorizationPolicy named: 'X' was not found.
/// for every permission that wasn't manually pre-registered in Program.cs.
/// </summary>
public class PermissionPolicyProvider(DefaultAuthorizationPolicyProvider _fallbackPolicyProvider) : IAuthorizationPolicyProvider
{
    // Falls back to the default provider for anything that IS explicitly
    // registered (e.g. named policies configured via options.AddPolicy
    // elsewhere that are NOT simple permission checks).


    // Delegates the default ([Authorize] with no policy name) policy to the framework.
    public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => _fallbackPolicyProvider.GetDefaultPolicyAsync();

    // Delegates the fallback (unauthenticated request) policy to the framework.
    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() => _fallbackPolicyProvider.GetFallbackPolicyAsync();

    // Resolves a named policy: checks the framework's explicitly registered
    // policies first (BUG FIX - the previous version never actually called
    // this and would have silently hijacked any future non-permission named
    // policy), and only synthesizes a PermissionRequirement as a fallback for
    // policy names that were never registered via options.AddPolicy.
    public async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        var explicitPolicy = await _fallbackPolicyProvider.GetPolicyAsync(policyName);
        if (explicitPolicy != null)
        {
            return explicitPolicy;
        }

        // Treat every other policy name coming from [HasPermission("...")] as
        // a permission check, by wrapping it in a PermissionRequirement.
        return new AuthorizationPolicyBuilder().AddRequirements(new PermissionRequirement(policyName)).Build();
    }
}