namespace LTSFrontend.Features.SuperAdminRequests.Services
{

    public sealed record SuperAdminRequestEligibility(bool CanSubmit, string? BlockedReason);










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


            return new SuperAdminRequestEligibility(true, null);
        }

        private static bool IsActive(string? status) =>
            string.Equals(status, Pending, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(status, Approved, StringComparison.OrdinalIgnoreCase);
    }
}
