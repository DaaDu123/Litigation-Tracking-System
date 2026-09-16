namespace LTSFrontend.Features.CaseParties.DTOs
{
    /// <summary>Mirrors LTSBackend.Features.CaseParties.DTOs.CasePartyDetailDTO</summary>
    public class CasePartyDTO
    {
        public long PartyID { get; set; }
        public long CaseID { get; set; }
        public string PartyType { get; set; } = string.Empty;
        public string PartyName { get; set; } = string.Empty;
        public string? Organization { get; set; }
        public string? CNIC { get; set; }
        public string? NTN { get; set; }
        public string? ContactNo { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? LawyerName { get; set; }
        public string? Remarks { get; set; }
    }
}
