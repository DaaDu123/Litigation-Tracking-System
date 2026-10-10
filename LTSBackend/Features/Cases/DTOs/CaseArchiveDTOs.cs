using System.ComponentModel.DataAnnotations;

namespace LTSBackend.Features.Cases.DTOs;

/// <summary>Optional reason recorded in the audit log when a case is archived.</summary>
public class ArchiveCaseDTO
{
    [MaxLength(500)]
    public string? Reason { get; set; }
}

/// <summary>Confirmation payload for permanently deleting an already-archived case.</summary>
public class PermanentDeleteCaseDTO
{
    /// <summary>The exact case number, typed by the user as a safety confirmation.</summary>
    [Required]
    public string ConfirmCaseNumber { get; set; } = string.Empty;

    [Required, MinLength(5), MaxLength(500)]
    public string Reason { get; set; } = string.Empty;
}
