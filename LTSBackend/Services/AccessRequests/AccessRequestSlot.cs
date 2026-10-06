using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using LTSBackend.Models.Security;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Services.AccessRequests;

/// <summary>
/// Enforces "one access-request target per user" at the DATABASE level via
/// the UserAccessRequestSlots primary key (see <see cref="UserAccessRequestSlot"/>).
/// Used by the submit / cancel / approve / reject handlers of both
/// UserJoinRequests (to a FirmAdmin) and FirmAdminRequests (to the SuperAdmin).
/// </summary>
public static class AccessRequestSlot
{
    public const string ConflictMessage = "You already have an active access request. You can request access from only ONE Firm Admin or ONE Super Admin at a time - cancel your existing request first.";

    /// <summary>
    /// Stages (does NOT save) the slot for this user. Call it right before the
    /// SaveChanges that inserts the request, so both rows commit atomically.
    /// A leftover slot with no Pending request behind it (stale) is healed
    /// first; a live one is refused with a friendly error. If two submissions
    /// race, the PK makes the loser's SaveChanges fail - catch it with
    /// <see cref="IsSlotConflict"/>.
    /// </summary>
    public static async Task AcquireAsync(AppDbContext context, int userId, string targetType, CancellationToken ct)
    {
        var existing = await context.UserAccessRequestSlots.AsNoTracking().FirstOrDefaultAsync(x => x.UserID == userId, ct);

        if (existing != null)
        {
            // IgnoreQueryFilters: UserJoinRequests is tenant-filtered and this user has no firm yet.
            bool live = existing.TargetType == UserAccessRequestSlot.TargetFirmAdmin
                ? await context.UserJoinRequests.AsNoTracking().IgnoreQueryFilters().AnyAsync(x => x.UserID == userId && x.Status == "Pending", ct)
                : await context.FirmAdminRequests.AsNoTracking().AnyAsync(x => x.UserID == userId && x.Status == "Pending", ct);

            if (live)
                throw new ValidationException([ConflictMessage]);

            // Stale slot (no Pending request behind it). Delete exactly this row
            // (matched by CreatedAtUtc) so we can never delete a fresh slot that a
            // concurrent request just created.
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

    /// <summary>
    /// Stages (does NOT save) removal of the user's slot of this type. Call it in
    /// the same SaveChanges that moves the request out of Pending.
    /// </summary>
    public static async Task ReleaseAsync(AppDbContext context, int? userId, string targetType, CancellationToken ct)
    {
        if (userId is null)
            return;

        var slot = await context.UserAccessRequestSlots.FirstOrDefaultAsync(x => x.UserID == userId.Value && x.TargetType == targetType, ct);

        if (slot != null)
            context.UserAccessRequestSlots.Remove(slot);
    }

    /// <summary>True when the exception is the primary-key violation on UserAccessRequestSlots (duplicate slot = lost race).</summary>
    public static bool IsSlotConflict(DbUpdateException ex) =>
        ex.InnerException is SqlException sql
        && (sql.Number == 2627 || sql.Number == 2601)
        && sql.Message.Contains("UserAccessRequestSlots", StringComparison.OrdinalIgnoreCase);
}
