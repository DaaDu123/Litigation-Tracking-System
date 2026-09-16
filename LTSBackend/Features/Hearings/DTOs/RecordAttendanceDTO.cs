namespace LTSBackend.Features.Hearings.DTOs
{
    public class RecordAttendanceDTO
    {
        public long HearingId { get; set; }
        public int UserId { get; set; }
        public bool IsPresent { get; set; }
        public string? AttendanceRole { get; set; }
        public string? Remarks { get; set; }
    }
}
