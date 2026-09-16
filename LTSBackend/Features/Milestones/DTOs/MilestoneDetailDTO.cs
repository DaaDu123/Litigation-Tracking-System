namespace LTSBackend.Features.Milestones.DTOs
{
    public class MilestoneDetailDTO
    {
        public long MilestoneID { get; set; }
        public long CaseID { get; set; }
        public string? CaseNumber { get; set; }
        public string Milestone { get; set; } = string.Empty;
        public DateTime MilestoneDate { get; set; }
        public string? Description { get; set; }
        public bool IsCompleted { get; set; }
        public string? CompletedByName { get; set; }
        public DateTime? CompletedDate { get; set; }
    }
}
