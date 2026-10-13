using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.LoginHistory.DeleteLoginHistory;



















public class DeleteLoginHistoryHandler(AppDbContext context): IRequestHandler<DeleteLoginHistoryCommand, bool>
{





    public async Task<bool> Handle(DeleteLoginHistoryCommand request,CancellationToken cancellationToken)
    {
        var history = await context.LoginHistories.FirstOrDefaultAsync(x => x.LoginID == request.LoginID, cancellationToken);

        if (history is null)
            throw new NotFoundException("Login history not found.");

        context.LoginHistories.Remove(history);

        await context.SaveChangesAsync(cancellationToken);

        return true;
    }
}