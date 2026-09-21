using System.ComponentModel.DataAnnotations;

namespace LTSFrontend.Features.Users.DTOs
{
    /// <summary>Mirrors LTSBackend.Features.Users.Commands.SetAvailability.SetAvailabilityCommand</summary>
    public class SetAvailabilityRequest : IValidatableObject
    {
        public bool IsAvailable { get; set; } = true;

        [StringLength(500, ErrorMessage = "Reason cannot exceed 500 characters.")]
        public string? Reason { get; set; }

        [Range(0, 3650, ErrorMessage = "Days must be between 0 and 3650.")]
        public int Days { get; set; }

        [Range(0, 23, ErrorMessage = "Hours must be between 0 and 23.")]
        public int Hours { get; set; }

        [Range(0, 59, ErrorMessage = "Minutes must be between 0 and 59.")]
        public int Minutes { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!IsAvailable)
            {
                if (string.IsNullOrWhiteSpace(Reason))
                    yield return new ValidationResult("Inactive reason is required.", new[] { nameof(Reason) });

                if (Days <= 0 && Hours <= 0 && Minutes <= 0)
                    yield return new ValidationResult("Inactive duration is required and must be greater than zero.", new[] { nameof(Days) });
            }
        }
    }
}
