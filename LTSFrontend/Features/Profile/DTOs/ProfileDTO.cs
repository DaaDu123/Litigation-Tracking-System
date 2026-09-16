namespace LTSFrontend.Features.Profile.DTOs
{
    /// <summary>Mirrors LTSBackend.Features.Profile.DTOs.ProfileDTO</summary>
    public class ProfileDTO
    {
        public int UserID { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Department { get; set; }
        public string? ProfileImage { get; set; }
        public string? RoleName { get; set; }
    }
}
