namespace LTSBackend.Features.LoginHistory.DTOs;

public class MyLoginHistoryDTO
{
    public int LoginID { get; set; }

    public DateTime LoginTime { get; set; }

    public DateTime? LogoutTime { get; set; }

    public string? IPAddress { get; set; }

    public string? UserAgent { get; set; }

    public string Status { get; set; } = string.Empty;

    public bool IsLoggedOut { get; set; }
}