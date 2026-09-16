namespace LTSFrontend.Features.CaseParties.DTOs
{
    /// <summary>Shared form model for creating/updating a case party. PartyID stays 0 when creating.</summary>
    public class CasePartyFormDTO
    {
        public long PartyID { get; set; }
        public long CaseID { get; set; }
        public string PartyType { get; set; } = "Plaintiff";
        public string PartyName { get; set; } = string.Empty;
        public string? Organization { get; set; }
        public string? CNIC { get; set; }
        public string? NTN { get; set; }
        public string? ContactNo { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? LawyerName { get; set; }
        public string? Remarks { get; set; }

        public static readonly string[] PartyTypes =
        {
            "Plaintiff", "Defendant", "Petitioner", "Respondent", "Applicant", "Respondent Department"
        };
    }
}
