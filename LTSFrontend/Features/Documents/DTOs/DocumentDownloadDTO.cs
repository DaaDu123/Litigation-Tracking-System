namespace LTSFrontend.Features.Documents.DTOs
{




    public class DocumentDownloadDTO
    {
        public long DocumentID { get; set; }
        public string FileName { get; set; } = string.Empty;
        public byte[] FileBytes { get; set; } = Array.Empty<byte>();
        public string ContentType { get; set; } = "application/octet-stream";
        public long FileSize { get; set; }
    }
}
