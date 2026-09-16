using System.Security.Claims;
using LTSBackend.Models.Security;
using Microsoft.AspNetCore.Http;

namespace LTSBackend.Services.CurrentUser;

/// <summary>
/// Reads the caller's identity straight off their JWT claims for the
/// current HTTP request. Every property below is a live read of the
/// ClaimsPrincipal, not a cached/queried value — so it always reflects
/// whatever the current request's token actually says.
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly ClaimsPrincipal? _user;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _user = httpContextAccessor.HttpContext?.User;
    }

    public bool IsAuthenticated => _user?.Identity?.IsAuthenticated == true;

    public int? UserID
    {
        get
        {
            var value = _user?.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(value, out var id) ? id : null;
        }
    }

    public int? FirmID
    {
        get
        {
            var value = _user?.FindFirstValue("FirmID");
            return int.TryParse(value, out var id) ? id : null;
        }
    }

    public string? RoleName => _user?.FindFirstValue(ClaimTypes.Role);

    public bool IsSuperAdmin => RoleName == RoleNames.SuperAdmin;
}
