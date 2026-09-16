namespace LTSFrontend.Core.DTOs
{
    public class ApiErrorEnvelope
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public List<string>? Errors { get; set; }
    }
}
