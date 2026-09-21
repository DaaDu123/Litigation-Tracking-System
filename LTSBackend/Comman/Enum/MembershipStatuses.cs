namespace LTSBackend.Comman.Enum;

/// <summary>
/// Distinct firm-membership states for a User, kept as separate named
/// states (not one generic boolean) per the Firm/User workflow spec:
///
///   Active  - normal, unrestricted member of their Firm (FirmID set).
///   Blocked - still attached to the Firm (FirmID stays set, so the
///             Firm Admin can still see/unblock the record) but every
///             API call is rejected by AccountStatusGuardBehavior. The
///             user cannot see or interact with the Firm in any way and
///             cannot re-request it while blocked.
///   Removed - detached from the Firm (FirmID cleared, LastFirmID keeps
///             a record of which Firm). Free to search for and request
///             membership with another Firm (or, per business rule, the
///             same Firm again later).
///
/// Stored as a plain string column (matching the existing Status string
/// convention already used on FirmAdminRequest/UserJoinRequest, e.g.
/// "Pending"/"Approved"/"Rejected") rather than an int enum, so it reads
/// the same way in the database and in ad-hoc queries as those tables do.
/// </summary>
public static class MembershipStatuses
{
    public const string Active = "Active";
    public const string Blocked = "Blocked";
    public const string Removed = "Removed";
}
