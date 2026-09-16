namespace LTSBackend.Features.DocumentTypes.DTOs;

/// <summary>
/// Minimal shape for populating a "Document Type" dropdown at upload time.
/// Deliberately excludes Description/IsActive/IsGlobal and everything else
/// on DocumentTypeDTO — this endpoint is NOT a master-data admin surface,
/// it only exists so AssociateLawyer/Moharrir/InternParalegal (who can
/// upload documents but cannot manage DocumentType master data) have a way
/// to fill that dropdown. See GetDocumentTypeOptionsHandler.
/// </summary>
public class DocumentTypeOptionDTO
{
    public int DocumentTypeID { get; set; }
    public string TypeName { get; set; } = string.Empty;
}
