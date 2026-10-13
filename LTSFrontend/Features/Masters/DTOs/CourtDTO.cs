namespace LTSFrontend.Features.Masters.DTOs
{

    public class CourtDTO
    {
        public int CourtID { get; set; }
        public string CourtName { get; set; } = string.Empty;
        public string? CourtType { get; set; }
        public string? Jurisdiction { get; set; }
        public string? Address { get; set; }
        public bool IsActive { get; set; }


        public bool IsGlobal { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
