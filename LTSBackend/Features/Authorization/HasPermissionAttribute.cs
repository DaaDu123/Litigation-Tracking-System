using Microsoft.AspNetCore.Authorization;
namespace LTSBackend.Features.Authorization;

public class HasPermissionAttribute : AuthorizeAttribute
{
    public HasPermissionAttribute(string permission)
    {
        Policy = permission;
    }
}