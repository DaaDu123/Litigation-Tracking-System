namespace LTSFrontend.Features.CaseAssignments.DTOs
{
    /// <summary>
    /// Form model for assigning a lawyer/counsel to a case. Also holds a static
    /// list of valid AssignmentType values (must match backend AssignCaseValidator).
    /// </summary>
    public class AssignCaseDTO
    {
        public long CaseID { get; set; }
        public int? UserID { get; set; }
        public string AssignmentType { get; set; } = "Lawyer";
        public bool LeadCounsel { get; set; }
        public string? Remarks { get; set; }

        public static readonly string[] AssignmentTypes =
        {
            "Legal Officer", "Supervisor", "Lawyer", "External Counsel"
        };
    }
}
