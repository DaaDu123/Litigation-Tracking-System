namespace LTSBackend.Features.Deadlines.DTOs
{
    public class CreateDeadlineDTO
    {
        public long CaseID { get; set; }
        public string DeadlineType { get; set; } = string.Empty; // Filing Reply, Appeal, Evidence Submission, etc.
        public DateTime DueDate { get; set; }
        public int ReminderDays { get; set; } = 7;
        public string? Remarks { get; set; }
    }
}
