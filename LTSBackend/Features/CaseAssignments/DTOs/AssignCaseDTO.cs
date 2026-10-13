namespace LTSBackend.Features.CaseAssignments.DTOs
{
    public class AssignCaseDTO
    {
        public long CaseID { get; set; }
        public int UserID { get; set; }
        public string AssignmentType { get; set; } = string.Empty;
        public bool LeadCounsel { get; set; }
        public string? Remarks { get; set; }
    }
}
