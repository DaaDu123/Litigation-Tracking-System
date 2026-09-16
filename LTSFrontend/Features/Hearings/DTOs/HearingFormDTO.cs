namespace LTSFrontend.Features.Hearings.DTOs
{
    /// <summary>Shared form model for creating/updating a hearing. HearingId stays 0 when creating.</summary>
    public class HearingFormDTO
    {
        public long HearingId { get; set; }
        public long CaseId { get; set; }
        public int? CourtId { get; set; }
        public DateTime? HearingDate { get; set; } = DateTime.Today.AddDays(7);
        public string? CourtRoom { get; set; }
        public string? JudgeName { get; set; }
        public string? HearingPurpose { get; set; }
        public string? HearingOutcome { get; set; }
        public DateTime? NextHearingDate { get; set; }
        public string? Remarks { get; set; }
    }
}
