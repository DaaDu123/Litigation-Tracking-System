namespace LTSBackend.Features.Deadlines.DTOs
{
    public class UpdateDeadlineDTO
    {
        public long DeadlineID { get; set; }
        public string DeadlineType { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public int ReminderDays { get; set; }
        public string? Remarks { get; set; }
    }
}
