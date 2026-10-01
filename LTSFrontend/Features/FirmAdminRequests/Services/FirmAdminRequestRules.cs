using LTSFrontend.Features.FirmAdminRequests.DTOs;

namespace LTSFrontend.Features.FirmAdminRequests.Services
{    
    public static class FirmAdminRequestRules
    {
        public const string PendingMessage = "Your request is awaiting approval.";
        public const string ApprovedMessage = "Your request has been approved.";
        public const string RejectedMessage = "Your previous request was rejected. You can submit a new request.";

        public static FirmRequestEligibility Evaluate(FirmAdminRequestDTO? latestFirmRequest, string? superAdminRequestStatus)
        {
            var phase = ToPhase(latestFirmRequest?.Status);

            // 1. A Firm Admin request that is already pending/approved always wins.
            if (phase == FirmRequestPhase.Pending)
            {
                return new FirmRequestEligibility(phase, false, false,
                    "Request pending",
                    PendingMessage,
                    "You'll get an email once a Super Admin reviews it. Until then your account stays a regular user account.",
                    latestFirmRequest);
            }

            if (phase == FirmRequestPhase.Approved)
            {
                return new FirmRequestEligibility(phase, false, false,
                    "Request approved",
                    ApprovedMessage,
                    "Sign out and sign back in to load your Firm Admin access, then finish setting up your firm.",
                    latestFirmRequest);
            }

            // 2. The other admin path (Super Admin) is occupied -> block.
            if (AdminRequestStatuses.OccupiesAdminPath(superAdminRequestStatus))
            {
                var state = string.Equals(superAdminRequestStatus, AdminRequestStatuses.Approved, StringComparison.OrdinalIgnoreCase)
                    ? "approved"
                    : "pending";

                return new FirmRequestEligibility(phase, false, true,
                    "Firm request unavailable",
                    $"You have a {state} Super Admin request, so you can't request a firm right now.",
                    "Only one admin request can be pending or approved at a time.",
                    latestFirmRequest);
            }

            // 3. Previous Firm Admin request was rejected -> may submit a brand-new one.
            if (phase == FirmRequestPhase.Rejected)
            {
                return new FirmRequestEligibility(phase, true, false,
                    "Request rejected",
                    RejectedMessage,
                    null,
                    latestFirmRequest);
            }

            // 4. Nothing yet.
            return new FirmRequestEligibility(FirmRequestPhase.NoRequest, true, false,
                "Create your own firm",
                "Request Firm Admin access to set up and manage your own firm workspace.",
                null,
                null);
        }

        private static FirmRequestPhase ToPhase(string? status)
        {
            if (string.Equals(status, AdminRequestStatuses.Pending, StringComparison.OrdinalIgnoreCase)) return FirmRequestPhase.Pending;
            if (string.Equals(status, AdminRequestStatuses.Approved, StringComparison.OrdinalIgnoreCase)) return FirmRequestPhase.Approved;
            if (string.Equals(status, AdminRequestStatuses.Rejected, StringComparison.OrdinalIgnoreCase)) return FirmRequestPhase.Rejected;
            return FirmRequestPhase.NoRequest;
        }
    }
}
