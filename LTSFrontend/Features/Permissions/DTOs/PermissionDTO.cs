namespace LTSFrontend.Features.Permissions.DTOs
{
    /// <summary>Mirrors LTSBackend.Features.Permissions.DTOs.PermissionDTO</summary>
    public class PermissionDTO
    {
        public int PermissionID { get; set; }
        public string PermissionName { get; set; } = string.Empty;
    }
}
