namespace LTSBackend.Features.CaseAssignments.DTOs
{
    public class CaseAssignmentDetailDTO
    {
        public long AssignmentID { get; set; }
        public long CaseID { get; set; }
        public string? CaseNumber { get; set; }
        public string? CaseTitle { get; set; }
        public int UserID { get; set; }
        public string? UserName { get; set; }
        public string? UserEmail { get; set; }
        public string AssignmentType { get; set; } = string.Empty;
        public bool LeadCounsel { get; set; }
        public DateTime AssignedDate { get; set; }
        public string? AssignedByName { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive => EndDate == null;
        public string? Remarks { get; set; }
    }
}
