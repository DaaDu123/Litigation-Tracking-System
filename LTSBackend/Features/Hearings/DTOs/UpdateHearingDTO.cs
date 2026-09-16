using System;
namespace LTSBackend.Features.Hearings.DTOs
{
    public class UpdateHearingDTO
    {
        public long HearingId { get; set; }
        public DateTime HearingDate { get; set; }
        public string? CourtRoom { get; set; }
        public string? JudgeName { get; set; }
        public string? HearingPurpose { get; set; }
        public string? HearingOutcome { get; set; }
        public DateTime? NextHearingDate { get; set; }
        public string? Remarks { get; set; }
    }
}