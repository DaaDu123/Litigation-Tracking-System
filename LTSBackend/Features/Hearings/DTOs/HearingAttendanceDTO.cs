using System;
namespace LTSBackend.Features.Hearings.DTOs
{
    public class HearingAttendanceDTO
    {
        public long AttendanceId { get; set; }
        public long HearingId { get; set; }
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public string? AttendanceRole { get; set; }
        public bool IsPresent { get; set; }
        public DateTime? ArrivalTime { get; set; }
        public DateTime? DepartureTime { get; set; }
        public string? Remarks { get; set; }
    }
}