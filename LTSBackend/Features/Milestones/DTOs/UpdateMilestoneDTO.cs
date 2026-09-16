namespace LTSBackend.Features.Milestones.DTOs
{
    public class UpdateMilestoneDTO
    {
        public long MilestoneID { get; set; }
        public string Milestone { get; set; } = string.Empty;
        public DateTime MilestoneDate { get; set; }
        public string? Description { get; set; }
    }
}
