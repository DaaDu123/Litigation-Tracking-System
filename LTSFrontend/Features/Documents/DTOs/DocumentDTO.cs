namespace LTSFrontend.Features.Documents.DTOs
{
    /// <summary>Mirrors LTSBackend.Features.Documents.DTOs.DocumentDetailDTO</summary>
    public class DocumentDTO
    {
        public long DocumentID { get; set; }
        public long CaseID { get; set; }
        public string CaseNumber { get; set; } = string.Empty;
        public string DocumentName { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string DocumentType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public int VersionNo { get; set; }
        public bool IsLatest { get; set; }
        public string UploadedBy { get; set; } = string.Empty;
        public DateTime UploadedDate { get; set; }
        public string Remarks { get; set; } = string.Empty;

        // Draft workflow (SRS - Intern/Paralegal): true until a Partner/
        // FirmAdmin approves the document via POST /api/documents/{id}/approve.
        public bool IsDraft { get; set; }
        public string? ApprovedByName { get; set; }
        public DateTime? ApprovedDate { get; set; }

        public string FormattedFileSize => FileSize switch
        {
            < 1024 => $"{FileSize} B",
            < 1024 * 1024 => $"{FileSize / 1024.0:F2} KB",
            < 1024 * 1024 * 1024 => $"{FileSize / (1024.0 * 1024):F2} MB",
            _ => $"{FileSize / (1024.0 * 1024 * 1024):F2} GB"
        };
    }
}
