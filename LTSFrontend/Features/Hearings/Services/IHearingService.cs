using LTSFrontend.Features.Hearings.DTOs;

namespace LTSFrontend.Features.Hearings.Services
{
    public interface IHearingService
    {
        Task<PagedHearingResult<HearingDTO>> GetByCaseAsync(long caseId, int pageNumber = 1, int pageSize = 10);
        Task<PagedHearingResult<HearingDTO>> GetUpcomingAsync(int pageNumber = 1, int pageSize = 10, long? caseId = null, int? courtId = null);
        Task<HearingDTO?> GetByIdAsync(long id);
        Task<long> CreateAsync(HearingFormDTO form);
        Task<bool> UpdateAsync(HearingFormDTO form);
        Task<bool> DeleteAsync(long id);

        // Hearing Attendance (Complete Database Schema - HearingAttendance table;
        // backend endpoints existed with no frontend caller until now)
        Task<List<HearingAttendanceDTO>> GetAttendanceAsync(long hearingId);
        Task<long> RecordAttendanceAsync(AttendanceFormDTO form);
        Task<bool> UpdateAttendanceAsync(AttendanceFormDTO form);
        Task<bool> DeleteAttendanceAsync(long hearingId, long attendanceId);
    }
}
