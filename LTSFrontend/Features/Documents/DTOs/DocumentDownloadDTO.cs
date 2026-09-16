namespace LTSFrontend.Features.Documents.DTOs
{
    /// <summary>
    /// Typed shape for a downloaded document's bytes (not currently used by
    /// DocumentService.DownloadAsync, which streams the response directly).
    /// </summary>
    public class DocumentDownloadDTO
    {
        public long DocumentID { get; set; }
        public string FileName { get; set; } = string.Empty;
        public byte[] FileBytes { get; set; } = Array.Empty<byte>();
        public string ContentType { get; set; } = "application/octet-stream";
        public long FileSize { get; set; }
    }
}
