namespace LTSBackend.Features.Roles.DTOs;

public class RoleDTO
{
    public int RoleID { get; set; }

    public string RoleName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public List<RolePermissionDTO> Permissions { get; set; } = [];
}