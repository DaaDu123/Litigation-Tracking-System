namespace LTSFrontend.Features.Users.DTOs
{
    /// <summary>Mirrors LTSBackend.Features.Users.DTOs.UserDTO</summary>
    public class UserDTO
    {
        public int UserID { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? ProfileImage { get; set; }
        public string? Phone { get; set; }
        public string? Department { get; set; }
        public int? RoleID { get; set; }
        public string? RoleName { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
