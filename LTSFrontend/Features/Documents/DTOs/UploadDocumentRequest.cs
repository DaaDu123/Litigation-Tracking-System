using Microsoft.AspNetCore.Components.Forms;

namespace LTSFrontend.Features.Documents.DTOs
{

    public class UploadDocumentRequest
    {
        public long CaseID { get; set; }
        public int? DocumentTypeID { get; set; }
        public string DocumentName { get; set; } = string.Empty;
        public string? Remarks { get; set; }
        public IBrowserFile? File { get; set; }
    }
}
