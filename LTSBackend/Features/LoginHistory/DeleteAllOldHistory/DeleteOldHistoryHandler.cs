using LTSBackend.Data;
using LTSBackend.Features.LoginHistory.DeleteAllOldHistory;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.LoginHistory.Commands.DeleteOldHistory;

public class DeleteOldHistoryHandler(AppDbContext context) : IRequestHandler<DeleteOldHistoryCommand, int>
{





    public async Task<int> Handle(DeleteOldHistoryCommand request, CancellationToken cancellationToken)
    {
        var cutOffDate = DateTime.UtcNow.AddDays(-request.Days);






        return await context.LoginHistories
            .Where(x => x.IsLoggedOut && x.LoginTime < cutOffDate)
            .ExecuteDeleteAsync(cancellationToken);
    }
}