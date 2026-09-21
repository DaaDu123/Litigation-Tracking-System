namespace LTSFrontend.Features.Profile.DTOs
{
    /// <summary>Mirrors LTSBackend.Features.Profile.DTOs.ProfileCompletionResultDTO</summary>
    public class ProfileCompletionResultDTO
    {
        public int UserID { get; set; }
        public string FullName { get; set; } = string.Empty;
        public bool IsProfileCompleted { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
