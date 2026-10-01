namespace LTSFrontend.Features.SuperAdminRequests.DTOs
{
    /// <summary>
    /// Read model for the caller's own Super Admin request.
    ///
    /// NOTE: the current LTSFrontend has no screen that *submits* a Super
    /// Admin request, so this slice is read-only for now. It exists so the
    /// "only one admin request path at a time" rule can be enforced from the
    /// Firm Admin side (see FirmAdminRequestRules) the moment the backend
    /// exposes GET /api/superadminrequests/mine. Until then
    /// <see cref="Services.ISuperAdminRequestService.GetMineAsync"/> simply
    /// returns null ("no request") and nothing is blocked.
    /// </summary>
    public class SuperAdminRequestDTO
    {
        public int RequestID { get; set; }

        /// <summary>Pending, Approved or Rejected.</summary>
        public string Status { get; set; } = string.Empty;

        public DateTime RequestedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string? RejectionReason { get; set; }
    }
}
