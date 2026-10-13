using LTSBackend.Data;
using LTSBackend.Features.Deadlines.DTOs;
using LTSBackend.Services.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.Deadlines.Queries.GetUpcomingDeadlines
{
    public class GetUpcomingDeadlinesHandler(AppDbContext _context, ICurrentUserService _currentUser) : IRequestHandler<GetUpcomingDeadlinesQuery, List<DeadlineDetailDTO>>
    {








        public async Task<List<DeadlineDetailDTO>> Handle(GetUpcomingDeadlinesQuery request, CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow.Date;

            var query = _context.Deadlines.AsNoTracking().Include(d => d.Case).Where(d => !d.Completed);


                query = query.Where(d => d.Case.FirmID == _currentUser.FirmID);

            var deadlines = await query.OrderBy(d => d.DueDate).ToListAsync(cancellationToken);

            var result = deadlines.Where(d =>
                {
                    var reminderDate = d.DueDate.Date.AddDays(-(request.DaysAhead ?? d.ReminderDays));
                    return now >= reminderDate;
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