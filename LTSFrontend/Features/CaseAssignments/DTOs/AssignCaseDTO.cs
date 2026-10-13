namespace LTSFrontend.Features.CaseAssignments.DTOs
{




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
