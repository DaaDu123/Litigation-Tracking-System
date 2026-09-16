namespace LTSFrontend.Features.Deadlines.DTOs
{
    /// <summary>Shared form model for creating/updating a deadline. DeadlineID stays 0 when creating.</summary>
    public class DeadlineFormDTO
    {
        public long DeadlineID { get; set; }
        public long CaseID { get; set; }
        public string DeadlineType { get; set; } = string.Empty;
        public DateTime? DueDate { get; set; } = DateTime.Today.AddDays(14);
        public int ReminderDays { get; set; } = 7;
        public string? Remarks { get; set; }

        public static readonly string[] CommonTypes =
        {
            "Filing Reply", "Appeal", "Evidence Submission", "Written Statement", "Rejoinder", "Compliance", "Other"
        };
    }
}
