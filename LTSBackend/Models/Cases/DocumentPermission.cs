using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LTSBackend.Models.Security;

namespace LTSBackend.Models.Cases;

[Table("DocumentPermissions")]
public class DocumentPermission
{
    [Key]
    public long PermissionID { get; set; }

    [Required]
    public long DocumentID { get; set; }

    // 🔴 MODIFIED: Role-based OR User-based permissions
    public int? RoleID { get; set; }
    public int? UserID { get; set; }

    public bool CanView { get; set; } = true;
    public bool CanDownload { get; set; } = false;
    public bool CanUpload { get; set; } = false;

    public DateTime? GrantedDate { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(DocumentID))]
    public Document Document { get; set; } = null!;

    [ForeignKey(nameof(RoleID))]
    public Role? Role { get; set; }

    [ForeignKey(nameof(UserID))]
    public User? User { get; set; }
}