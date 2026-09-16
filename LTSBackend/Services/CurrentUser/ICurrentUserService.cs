namespace LTSBackend.Services.CurrentUser;
public interface ICurrentUserService
{
    int? UserID { get; }
    int? FirmID { get; }
    string? RoleName { get; }
    bool IsSuperAdmin { get; }
    bool IsAuthenticated { get; }
}
