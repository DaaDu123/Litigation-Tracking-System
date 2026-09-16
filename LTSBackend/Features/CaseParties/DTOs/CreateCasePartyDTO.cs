namespace LTSBackend.Features.CaseParties.DTOs
{
    public class CreateCasePartyDTO
    {
        public long CaseID { get; set; }
        public string PartyType { get; set; } = string.Empty; // Plaintiff, Defendant, Petitioner, Respondent, Applicant, Respondent Department
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
