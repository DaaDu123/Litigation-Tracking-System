using System.ComponentModel.DataAnnotations;

namespace LTSFrontend.Features.Marketing.DTOs
{
    /// <summary>Mirrors LTSBackend.Features.ContactMessages.Commands.SubmitContactMessage.SubmitContactMessageCommand</summary>
    public class SubmitContactMessage
    {
        [Required(ErrorMessage = "Please enter your name.")]
        [StringLength(150, ErrorMessage = "Name cannot exceed 150 characters.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter your email.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        [StringLength(200, ErrorMessage = "Email cannot exceed 200 characters.")]
        public string Email { get; set; } = string.Empty;

        [StringLength(30, ErrorMessage = "Phone cannot exceed 30 characters.")]
        [RegularExpression(@"^\+?[0-9\-\(\)\s]*$", ErrorMessage = "Phone format is invalid.")]
        public string? Phone { get; set; }

        [Required(ErrorMessage = "Please enter a message.")]
        [StringLength(2000, ErrorMessage = "Message cannot exceed 2000 characters.")]
        public string Message { get; set; } = string.Empty;
    }
}
