using LTSBackend.Models.Security;

namespace LTSBackend.Services.Availability;

public record EffectiveAvailability(bool IsAvailable, string? Reason, DateTime? InactiveFromUtc, DateTime? InactiveUntilUtc);

/// <summary>
/// Single source of truth for "is this Firm Admin currently available".
/// Used both by FirmAdminAvailabilityReactivationService (the background
/// sweep) and by every read path (GetFirmAdminAvailabilityQuery, the
/// Firm Users directory, GetJoinableFirms) so an expired InactiveUntilUtc
/// is treated as Active immediately, even if the sweep hasn't run yet -
/// per spec §9, the system must handle expired records correctly even
/// when the background service is behind.
/// </summary>
public static class AvailabilityEvaluator
{
    public static EffectiveAvailability GetEffective(User user)
    {
        if (user.IsAvailable)
            return new EffectiveAvailability(true, null, null, null);

        // Marked inactive, but the window has already passed -> effectively active.
        if (user.InactiveUntilUtc.HasValue && user.InactiveUntilUtc.Value <= DateTime.UtcNow)
            return new EffectiveAvailability(true, null, null, null);

        return new EffectiveAvailability(false, user.InactiveReason, user.InactiveFromUtc, user.InactiveUntilUtc);
    }
}
