namespace LTSFrontend.Features.SuperAdminRequests.Services
{
    /// <summary>Outcome of the "may this user submit a Super Admin request?" rule.</summary>
    public sealed record SuperAdminRequestEligibility(bool CanSubmit, string? BlockedReason);

    /// <summary>
    /// Mirror image of FirmAdminRequestRules: a user can only be on ONE admin
    /// path. Whichever screen eventually submits Super Admin requests should
    /// call <see cref="Evaluate"/> with the user's latest Firm Admin request
    /// status and hide/disable its submit action when CanSubmit is false.
    ///
    /// Takes plain status strings (not the Firm Admin DTO) so this slice never
    /// depends on the FirmAdminRequests slice.
    /// </summary>
    public static class SuperAdminRequestRules
    {
        private const string Pending = "Pending";
        private const string Approved = "Approved";

        public static SuperAdminRequestEligibility Evaluate(string? latestSuperAdminStatus, string? latestFirmAdminStatus)
        {
            if (IsActive(latestSuperAdminStatus))
            {
                return new SuperAdminRequestEligibility(false,
                    string.Equals(latestSuperAdminStatus, Approved, StringComparison.OrdinalIgnoreCase)
                        ? "Your Super Admin request has been approved."
                        : "Your Super Admin request is awaiting approval.");
            }

            if (IsActive(latestFirmAdminStatus))
            {
                return new SuperAdminRequestEligibility(false,
                    $"You have a {latestFirmAdminStatus!.ToLowerInvariant()} Firm Admin request, so you can't request Super Admin access. " +
                    "Only one admin request can be pending or approved at a time.");
            }

            // No request, or the previous one was rejected -> a NEW request may be submitted.
            return new SuperAdminRequestEligibility(true, null);
        }

        private static bool IsActive(string? status) =>
            string.Equals(status, Pending, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(status, Approved, StringComparison.OrdinalIgnoreCase);
    }
}
