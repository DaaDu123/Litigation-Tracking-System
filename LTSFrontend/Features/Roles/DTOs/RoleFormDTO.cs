using System.ComponentModel.DataAnnotations;

namespace LTSFrontend.Features.Roles.DTOs
{






    public class RoleFormDTO
    {

        public int? RoleID { get; set; }

        [Required(ErrorMessage = "Role name is required.")]
        [StringLength(50, ErrorMessage = "Role name cannot exceed 50 characters.")]
        [RegularExpression(@"^[a-zA-Z0-9\s_-]+$",
            ErrorMessage = "Role name can only contain letters, numbers, spaces, hyphens, and underscores.")]
        public string RoleName { get; set; } = string.Empty;

        [StringLength(255, ErrorMessage = "Description cannot exceed 255 characters.")]
        public string? Description { get; set; }

        public HashSet<int> PermissionIds { get; set; } = new();

        public bool IsEditMode => RoleID.HasValue;

        public static RoleFormDTO FromDto(RoleDTO role) => new()
        {
            RoleID = role.RoleID,
            RoleName = role.RoleName,
            Description = role.Description,
            PermissionIds = role.Permissions.Select(p => p.PermissionID).ToHashSet()
        };
    }
}
