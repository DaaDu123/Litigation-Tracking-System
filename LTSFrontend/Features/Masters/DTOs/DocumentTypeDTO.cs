namespace LTSFrontend.Features.Masters.DTOs
{
    /// <summary>Mirrors LTSBackend.Features.DocumentTypes.DTOs.DocumentTypeDTO</summary>
    public class DocumentTypeDTO
    {
        public int DocumentTypeID { get; set; }
        public string TypeName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }

        /// <summary>True = system-wide record shared across all firms. Only SuperAdmin can edit/deactivate/delete these.</summary>
        public bool IsGlobal { get; set; }
    }
}
