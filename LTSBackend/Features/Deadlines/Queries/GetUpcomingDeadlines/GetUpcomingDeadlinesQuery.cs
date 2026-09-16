using MediatR;
using LTSBackend.Features.Deadlines.DTOs;

namespace LTSBackend.Features.Deadlines.Queries.GetUpcomingDeadlines
{
    /// <summary>
    /// SRS: FR-08 "System shall generate reminders and alerts"
    /// Returns deadlines that have entered their ReminderDays window and are not completed
    /// Also flags overdue deadlines
    /// </summary>
    public class GetUpcomingDeadlinesQuery : IRequest<List<DeadlineDetailDTO>>
    {
        public int? DaysAhead { get; set; } // optional override; default uses each deadline's own ReminderDays
    }
}
