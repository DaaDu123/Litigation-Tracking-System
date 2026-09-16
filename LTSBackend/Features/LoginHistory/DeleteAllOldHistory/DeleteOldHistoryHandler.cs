using LTSBackend.Data;
using LTSBackend.Features.LoginHistory.DeleteAllOldHistory;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.LoginHistory.Commands.DeleteOldHistory;

public class DeleteOldHistoryHandler(AppDbContext context) : IRequestHandler<DeleteOldHistoryCommand, int>
{
    // =====================================================
    // HANDLE — bulk retention sweep for old, logged-out login records
    // Removes every (tenant-scoped) logged-out record older than
    // request.Days and returns the count removed.
    // =====================================================
    public async Task<int> Handle(DeleteOldHistoryCommand request, CancellationToken cancellationToken)
    {
        var cutOffDate = DateTime.UtcNow.AddDays(-request.Days);

        // ROOT-CAUSE FIX (performance): previously loaded every matching
        // row into memory (ToListAsync) just to RemoveRange + SaveChanges
        // them - for a large old-record backlog that's thousands of
        // tracked entities for no reason. ExecuteDeleteAsync (EF Core 7+)
        // translates straight to a single SQL DELETE statement.
        return await context.LoginHistories
            .Where(x => x.IsLoggedOut && x.LoginTime < cutOffDate)
            .ExecuteDeleteAsync(cancellationToken);
    }
}