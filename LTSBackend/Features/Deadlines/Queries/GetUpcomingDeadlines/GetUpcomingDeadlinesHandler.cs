using LTSBackend.Data;
using LTSBackend.Features.Deadlines.DTOs;
using LTSBackend.Services.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.Deadlines.Queries.GetUpcomingDeadlines
{
    public class GetUpcomingDeadlinesHandler(AppDbContext _context, ICurrentUserService _currentUser) : IRequestHandler<GetUpcomingDeadlinesQuery, List<DeadlineDetailDTO>>
    {
        // =====================================================
        // HANDLE — deadlines due soon across the caller's own firm
        // Firm-scoped (previously leaked every firm's deadlines to any
        // logged-in user). A deadline is included once "today" falls
        // inside its reminder window — using the request's own
        // daysAhead override if supplied, else each deadline's own
        // ReminderDays — or once it's already overdue.
        // =====================================================
        public async Task<List<DeadlineDetailDTO>> Handle(GetUpcomingDeadlinesQuery request, CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow.Date;

            var query = _context.Deadlines.AsNoTracking().Include(d => d.Case).Where(d => !d.Completed);

            // FIX: previously this leaked EVERY firm's upcoming deadlines to any logged-in user
                query = query.Where(d => d.Case.FirmID == _currentUser.FirmID);

            var deadlines = await query.OrderBy(d => d.DueDate).ToListAsync(cancellationToken);

            var result = deadlines.Where(d =>
                {
                    var reminderDate = d.DueDate.Date.AddDays(-(request.DaysAhead ?? d.ReminderDays));
                    return now >= reminderDate; // inside reminder window OR overdue
                })
                .Select(d => new DeadlineDetailDTO
                {
                    DeadlineID = d.DeadlineID,
                    CaseID = d.CaseID,
                    CaseNumber = d.Case?.CaseNumber,
                    CaseTitle = d.Case?.CaseTitle,
                    DeadlineType = d.DeadlineType,
                    DueDate = d.DueDate,
                    ReminderDays = d.ReminderDays,
                    Completed = d.Completed,
                    CompletedDate = d.CompletedDate,
                    Remarks = d.Remarks,
                    DaysRemaining = (int)(d.DueDate.Date - now).TotalDays,
                    IsOverdue = d.DueDate.Date < now
                })
                .ToList();

            return result;
        }
    }
}