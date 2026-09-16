namespace LTSBackend.Features.Milestones.DTOs
{
    public class CreateMilestoneDTO
    {
        public long CaseID { get; set; }
        public string Milestone { get; set; } = string.Empty;
        public DateTime MilestoneDate { get; set; }
        public string? Description { get; set; }
    }
}
