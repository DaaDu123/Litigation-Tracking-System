namespace LTSFrontend.Features.Documents.DTOs
{
    /// <summary>Mirrors LTSBackend.Features.Documents.Commands.UploadDocument.UploadDocumentResponseDTO</summary>
    public class UploadDocumentResponseDTO
    {
        public long DocumentID { get; set; }
        public long CaseID { get; set; }
        public string DocumentName { get; set; } = string.Empty;
        public bool IsRestrictedMohallirUpload { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
