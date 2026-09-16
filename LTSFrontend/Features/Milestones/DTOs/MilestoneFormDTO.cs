namespace LTSFrontend.Features.Milestones.DTOs
{
    /// <summary>Shared form model for creating/updating a milestone. MilestoneID stays 0 when creating.</summary>
    public class MilestoneFormDTO
    {
        public long MilestoneID { get; set; }
        public long CaseID { get; set; }
        public string Milestone { get; set; } = string.Empty;
        public DateTime? MilestoneDate { get; set; } = DateTime.Today;
        public string? Description { get; set; }
    }
}
