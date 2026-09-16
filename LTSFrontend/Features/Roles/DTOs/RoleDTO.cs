namespace LTSFrontend.Features.Roles.DTOs
{
    /// <summary>Mirrors LTSBackend.Features.Roles.DTOs.RoleDTO</summary>
    public class RoleDTO
    {
        public int RoleID { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public List<RolePermissionDTO> Permissions { get; set; } = new();
    }
}
