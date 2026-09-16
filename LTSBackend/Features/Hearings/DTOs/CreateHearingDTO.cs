using System;
namespace LTSBackend.Features.Hearings.DTOs
{
    public class CreateHearingDTO
    {
        public long CaseId { get; set; }
        public int CourtId { get; set; }
        public DateTime HearingDate { get; set; }
        public string? CourtRoom { get; set; }
        public string? JudgeName { get; set; }
        public string? HearingPurpose { get; set; }
        public string? Remarks { get; set; }
    }
}