using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using LTSBackend.Models.Security;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Services.AccessRequests;







public static class AccessRequestSlot
{
    public const string ConflictMessage = "You already have an active access request. You can request access from only ONE Firm Admin or ONE Super Admin at a time - cancel your existing request first.";









    public static async Task AcquireAsync(AppDbContext context, int userId, string targetType, CancellationToken ct)
    {
        var existing = await context.UserAccessRequestSlots.AsNoTracking().FirstOrDefaultAsync(x => x.UserID == userId, ct);

        if (existing != null)
        {

            bool live = existing.TargetType == UserAccessRequestSlot.TargetFirmAdmin
                ? await context.UserJoinRequests.AsNoTracking().IgnoreQueryFilters().AnyAsync(x => x.UserID == userId && x.Status == "Pending", ct)
                : await context.FirmAdminRequests.AsNoTracking().AnyAsync(x => x.UserID == userId && x.Status == "Pending", ct);

            if (live)
                throw new ValidationException([ConflictMessage]);




            await context.UserAccessRequestSlots
                .Where(x => x.UserID == userId && x.CreatedAtUtc == existing.CreatedAtUtc)
                .ExecuteDeleteAsync(ct);
        }

        context.UserAccessRequestSlots.Add(new UserAccessRequestSlot
        {
            UserID = userId,
            TargetType = targetType,
            CreatedAtUtc = DateTime.UtcNow
        });
    }





    public static async Task ReleaseAsync(AppDbContext context, int? userId, string targetType, CancellationToken ct)
    {
        if (userId is null)
            return;

        var slot = await context.UserAccessRequestSlots.FirstOrDefaultAsync(x => x.UserID == userId.Value && x.TargetType == targetType, ct);

        if (slot != null)
            context.UserAccessRequestSlots.Remove(slot);
    }


    public static bool IsSlotConflict(DbUpdateException ex) =>
        ex.InnerException is SqlException sql
        && (sql.Number == 2627 || sql.Number == 2601)
        && sql.Message.Contains("UserAccessRequestSlots", StringComparison.OrdinalIgnoreCase);
}
