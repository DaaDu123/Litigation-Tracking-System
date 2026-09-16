namespace LTSFrontend.Features.Users.DTOs
{
    /// <summary>Mirrors LTSBackend.Features.Users.DTOs.DeletedUserDTO</summary>
    public class DeletedUserDTO
    {
        public int UserID { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int? FirmID { get; set; }
        public string? FirmName { get; set; }
        public string? RoleName { get; set; }
        public bool IsReleasedForReuse { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
