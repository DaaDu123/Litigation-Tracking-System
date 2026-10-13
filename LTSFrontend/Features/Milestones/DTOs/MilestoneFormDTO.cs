namespace LTSFrontend.Features.Milestones.DTOs
{

    public class MilestoneFormDTO
    {
        public long MilestoneID { get; set; }
        public long CaseID { get; set; }
        public string Milestone { get; set; } = string.Empty;
        public DateTime? MilestoneDate { get; set; } = DateTime.Today;
        public string? Description { get; set; }
    }
}
