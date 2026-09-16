namespace LTSFrontend.Features.Hearings.DTOs
{
    /// <summary>
    /// Shared create/edit form model for one attendance row.
    /// AttendanceId == 0 means "record new"; otherwise "update existing".
    /// </summary>
    public class AttendanceFormDTO
    {
        public long AttendanceId { get; set; }
        public long HearingId { get; set; }
        public int UserId { get; set; }
        public string? AttendanceRole { get; set; }
        public bool IsPresent { get; set; } = true;
        public DateTime? ArrivalTime { get; set; }
        public DateTime? DepartureTime { get; set; }
        public string? Remarks { get; set; }

        public bool IsEditMode => AttendanceId > 0;
    }
}
