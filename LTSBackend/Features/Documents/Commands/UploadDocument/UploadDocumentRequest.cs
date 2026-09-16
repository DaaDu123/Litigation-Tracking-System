namespace LTSBackend.Features.Documents.Commands.UploadDocument
{
    public class UploadDocumentRequest
    {
        public long CaseID { get; set; }
        public int DocumentTypeID { get; set; }
        public string DocumentName { get; set; } = string.Empty;
        public IFormFile File { get; set; } = default!;
        public string? Remarks { get; set; }
    }
}
