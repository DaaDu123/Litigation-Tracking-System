namespace LTSBackend.Features.Documents.DTOs
{
    public class DocumentDetailDTO
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
        public bool IsDraft { get; set; }
        public string? ApprovedByName { get; set; }
        public DateTime? ApprovedDate { get; set; }

        public string FormattedFileSize => FormatFileSize(FileSize);

        // Converts a raw byte count into a human-readable size string (KB/MB/GB).
        private static string FormatFileSize(long bytes)
        {
            return bytes switch
            {
                < 1024 => $"{bytes} B",
                < 1024 * 1024 => $"{bytes / 1024.0:F2} KB",
                < 1024 * 1024 * 1024 => $"{bytes / (1024.0 * 1024):F2} MB",
                _ => $"{bytes / (1024.0 * 1024 * 1024):F2} GB"
            };
        }
    }
}
