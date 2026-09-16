using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.LoginHistory.DeleteLoginHistory;

/// <summary>
/// Deletes a single login history record by ID.
///
/// TENANT ISOLATION (fixed - see AppDbContext.cs): this used to look up the
/// record by LoginID alone with no ownership/tenant check at all - any user
/// holding the DeleteLoginHistory permission could delete any OTHER firm's
/// login history row just by guessing/incrementing LoginID (a cross-tenant
/// BOLA/IDOR). It is now safe by construction: the global query filter on
/// LoginHistory in AppDbContext means a cross-tenant LoginID simply cannot
/// be found by this query, so it correctly throws NotFoundException (404)
/// instead of ever touching another firm's data. Do NOT add
/// ".IgnoreQueryFilters()" here.
///
/// Currently only reachable by SuperAdmin in practice, since
/// DeleteLoginHistory is deliberately not granted to any other role (see
/// PermissionEnum.cs) - login history is a security-relevant audit trail
/// and the SRS requires audit logs to remain tamper-resistant.
/// </summary>
public class DeleteLoginHistoryHandler(AppDbContext context): IRequestHandler<DeleteLoginHistoryCommand, bool>
{
    // =====================================================
    // HANDLE — deletes one login history record by ID
    // Throws NotFoundException if it doesn't exist (or, thanks to the
    // tenant query filter, doesn't belong to the caller's firm).
    // =====================================================
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