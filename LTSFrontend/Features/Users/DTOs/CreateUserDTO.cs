using System.ComponentModel.DataAnnotations;

namespace LTSFrontend.Features.Users.DTOs
{
    /// <summary>
    /// Client-side form model for creating/updating a user. Sent to the
    /// backend as multipart/form-data (CreateUserCommand / UpdateUserCommand
    /// both bind via [FromForm] because of the optional ProfileImage file).
    /// Password has no [Required] here because it's blank/ignored on update -
    /// UserFormModal enforces "required on create" manually before submit.
    /// </summary>
    public class CreateUserDTO
    {
        public int? UserID { get; set; } // set when editing

        // Required in both Create and Edit — the Add User form collects
        // it directly from the Firm Admin; the backend no longer guesses it.
        [Required(ErrorMessage = "Full name is required.")]
        [StringLength(150, ErrorMessage = "Full name cannot exceed 150 characters.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        [StringLength(150, ErrorMessage = "Email cannot exceed 150 characters.")]
        public string Email { get; set; } = string.Empty;

        [StringLength(255, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters.")]
        public string Password { get; set; } = string.Empty; // ignored on update

        [StringLength(20, ErrorMessage = "Phone cannot exceed 20 characters.")]
        public string? Phone { get; set; }

        [StringLength(100, ErrorMessage = "Department cannot exceed 100 characters.")]
        public string? Department { get; set; }

        public int? RoleID { get; set; }
        public bool IsActive { get; set; } = true; // used on update only
    }
}
