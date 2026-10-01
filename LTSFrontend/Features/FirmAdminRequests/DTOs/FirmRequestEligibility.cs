namespace LTSFrontend.Features.FirmAdminRequests.DTOs
{
    /// <summary>Where a user stands in the Firm Admin request lifecycle: No Request -> Pending -> Approved / Rejected.</summary>
    public enum FirmRequestPhase
    {
        NoRequest,
        Pending,
        Approved,
        Rejected
    }

    /// <summary>Status strings used by the backend for admin requests.</summary>
    public static class AdminRequestStatuses
    {
        public const string Pending = "Pending";
        public const string Approved = "Approved";
        public const string Rejected = "Rejected";

        /// <summary>Pending or Approved - the two states that occupy the user's single admin path.</summary>
        public static bool OccupiesAdminPath(string? status) =>
            string.Equals(status, Pending, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(status, Approved, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// What the UI needs to render the "Create Your Own Firm" experience:
    /// the phase, whether a (new) request may be submitted right now, and
    /// the exact copy to show. Built only by <see cref="Services.FirmAdminRequestRules"/>.
    /// </summary>
    public sealed record FirmRequestEligibility(
        FirmRequestPhase Phase,
        bool CanSubmit,
        bool IsBlockedBySuperAdminRequest,
        string StatusTitle,
        string StatusMessage,
        string? HelpText,
        FirmAdminRequestDTO? LatestRequest);
}
