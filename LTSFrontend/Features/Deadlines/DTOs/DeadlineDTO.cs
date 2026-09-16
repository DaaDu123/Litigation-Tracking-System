namespace LTSFrontend.Features.Deadlines.DTOs
{
    /// <summary>Mirrors LTSBackend.Features.Deadlines.DTOs.DeadlineDetailDTO</summary>
    public class DeadlineDTO
    {
        public long DeadlineID { get; set; }
        public long CaseID { get; set; }
        public string? CaseNumber { get; set; }
        public string? CaseTitle { get; set; }
        public string DeadlineType { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public int ReminderDays { get; set; }
        public DateTime ReminderDate { get; set; }
        public bool Completed { get; set; }
        public DateTime? CompletedDate { get; set; }
        public string? Remarks { get; set; }
        public int DaysRemaining { get; set; }
        public bool IsOverdue { get; set; }
    }
}
