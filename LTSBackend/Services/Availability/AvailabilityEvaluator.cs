using LTSBackend.Models.Security;

namespace LTSBackend.Services.Availability;

public record EffectiveAvailability(bool IsAvailable, string? Reason, DateTime? InactiveFromUtc, DateTime? InactiveUntilUtc);










public static class AvailabilityEvaluator
{
    public static EffectiveAvailability GetEffective(User user)
    {
        if (user.IsAvailable)
            return new EffectiveAvailability(true, null, null, null);


        if (user.InactiveUntilUtc.HasValue && user.InactiveUntilUtc.Value <= DateTime.UtcNow)
            return new EffectiveAvailability(true, null, null, null);

        return new EffectiveAvailability(false, user.InactiveReason, user.InactiveFromUtc, user.InactiveUntilUtc);
    }
}
