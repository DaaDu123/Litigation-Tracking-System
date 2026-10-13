using System.ComponentModel.DataAnnotations;

namespace LTSBackend.Features.Cases.DTOs;


public class ArchiveCaseDTO
{
    [MaxLength(500)]
    public string? Reason { get; set; }
}


public class PermanentDeleteCaseDTO
{

    [Required]
    public string ConfirmCaseNumber { get; set; } = string.Empty;

    [Required, MinLength(5), MaxLength(500)]
    public string Reason { get; set; } = string.Empty;
}
