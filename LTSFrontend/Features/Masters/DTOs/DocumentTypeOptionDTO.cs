namespace LTSFrontend.Features.Masters.DTOs
{
    /// <summary>
    /// Mirrors LTSBackend.Features.DocumentTypes.DTOs.DocumentTypeOptionDTO.
    /// Minimal shape for the "Document Type" dropdown on the Upload
    /// Document form - used instead of the full DocumentTypeDTO/GetAllAsync
    /// because AssociateLawyer/Moharrir/InternParalegal can upload documents
    /// but cannot reach the master-data admin endpoints (FirmAdmin/Partner
    /// only). See DocumentTypeService.GetOptionsAsync.
    /// </summary>
    public class DocumentTypeOptionDTO
    {
        public int DocumentTypeID { get; set; }
        public string TypeName { get; set; } = string.Empty;
    }
}
