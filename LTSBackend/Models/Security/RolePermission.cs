using System.ComponentModel.DataAnnotations.Schema;

namespace LTSBackend.Models.Security
{
    public class RolePermission
    {
        public int RolePermissionID { get; set; }

        public int RoleID { get; set; }

        public int PermissionID { get; set; }

        public Role? Role { get; set; }

        public Permission? Permission { get; set; }
    }
}
