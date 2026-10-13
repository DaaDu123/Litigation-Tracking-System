using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace LTSBackend.Features.Authorization;










public class PermissionPolicyProvider(DefaultAuthorizationPolicyProvider _fallbackPolicyProvider) : IAuthorizationPolicyProvider
{






    public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => _fallbackPolicyProvider.GetDefaultPolicyAsync();


    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() => _fallbackPolicyProvider.GetFallbackPolicyAsync();






    public async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        var explicitPolicy = await _fallbackPolicyProvider.GetPolicyAsync(policyName);
        if (explicitPolicy != null)
        {
            return explicitPolicy;
        }



        return new AuthorizationPolicyBuilder().AddRequirements(new PermissionRequirement(policyName)).Build();
    }
}