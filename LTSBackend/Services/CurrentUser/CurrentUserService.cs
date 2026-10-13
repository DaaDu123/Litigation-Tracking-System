using System.Security.Claims;
using LTSBackend.Models.Security;
using Microsoft.AspNetCore.Http;

namespace LTSBackend.Services.CurrentUser;







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
