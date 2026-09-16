using System.ComponentModel.DataAnnotations;
namespace LTSBackend.Models.Security
{
    public class Permission
    {
        [Key]
        public int PermissionID { get; set; }
        [Required, MaxLength(100)]
        public string PermissionName { get; set; } = string.Empty;
        [MaxLength(255)]
        public string? Description { get; set; }
        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}
