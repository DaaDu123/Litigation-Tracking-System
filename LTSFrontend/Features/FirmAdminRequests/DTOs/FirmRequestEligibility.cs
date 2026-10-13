namespace LTSFrontend.Features.FirmAdminRequests.DTOs
{

    public enum FirmRequestPhase
    {
        NoRequest,
        Pending,
        Approved,
        Rejected
    }


    public static class AdminRequestStatuses
    {
        public const string Pending = "Pending";
        public const string Approved = "Approved";
        public const string Rejected = "Rejected";


        public static bool OccupiesAdminPath(string? status) =>
            string.Equals(status, Pending, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(status, Approved, StringComparison.OrdinalIgnoreCase);
    }






    public sealed record FirmRequestEligibility(
        FirmRequestPhase Phase,
        bool CanSubmit,
        bool IsBlockedBySuperAdminRequest,
        string StatusTitle,
        string StatusMessage,
        string? HelpText,
        FirmAdminRequestDTO? LatestRequest);
}
