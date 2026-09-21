namespace LTSBackend.Features.Profile.DTOs;

public class ProfileCompletionResultDTO
{
    public int UserID { get; set; }
    public string FullName { get; set; } = string.Empty;
    public bool IsProfileCompleted { get; set; }
    public string Message { get; set; } = string.Empty;
}
