using LTSBackend.Comman.Enum;

namespace LTSBackend.Models.Security;

public static class RoleNames
{
    public const string SuperAdmin = nameof(UserRole.SuperAdmin);
    public const string FirmAdmin = nameof(UserRole.FirmAdmin);
    public const string Partner = nameof(UserRole.Partner);
    public const string AssociateLawyer = nameof(UserRole.AssociateLawyer);
    public const string Moharrir = nameof(UserRole.Moharrir);
    public const string InternParalegal = nameof(UserRole.InternParalegal);

    // POLICY COMBINATIONS

    /// <summary>
    /// Super Admin only - system-wide management
    /// </summary>
    public const string SuperAdminOnly = SuperAdmin;

    /// <summary>
    /// Firm Admin only, with NO Partner. Reserved for the narrow set of
    /// actions that result in a brand-new user account being created
    /// (UsersController.Create, UserJoinRequestsController.Approve) — the
    /// one deliberate exception where Partner does NOT get the same access
    /// as FirmAdmin. Every other firm-level action should use
    /// FirmAdminAndAbove instead, which includes Partner.
    /// </summary>
    public const string FirmAdminOnly = FirmAdmin;

    /// <summary>
    /// Firm's senior management — FirmAdmin and Partner. Per policy, Partner
    /// has the SAME access as FirmAdmin across the firm (case management,
    /// master data administration, user administration, etc.) with exactly
    /// ONE carve-out: Partner cannot CREATE a new user account (see the
    /// explicit RoleNames.FirmAdminOnly override on UsersController's
    /// Create endpoint and UserJoinRequestsController's Approve endpoint,
    /// both of which result in a new user account and are deliberately
    /// kept FirmAdmin-only). Super Admin is deliberately EXCLUDED — firm-level
    /// management (master data, firm users, cases, documents) is entirely
    /// the firm's own domain, not the platform owner's. Kept the historical
    /// name (rather than renaming every call site).
    /// </summary>
    public const string FirmAdminAndAbove = FirmAdmin + "," + Partner;

    /// <summary>
    /// Partner + Firm Admin. Super Admin is deliberately EXCLUDED for the
    /// same reason as FirmAdminAndAbove above - senior firm management is
    /// not a platform-owner task.
    /// </summary>
    public const string PartnerAndAbove = Partner + "," + FirmAdmin;

    /// <summary>
    /// All lawyers - FirmAdmin, Partner, Associate, Moharrir with permissions.
   /// </summary>
    public const string AllLawyers = FirmAdmin + "," + Partner + "," + AssociateLawyer + "," + Moharrir;

    /// <summary>
    /// All staff except Super Admin
    /// </summary>
    public const string AllFirmUsers = FirmAdmin + "," + Partner + "," + AssociateLawyer + "," +
                                      Moharrir + "," + InternParalegal;

    /// <summary>
    /// REVERTED: this used to add Super Admin back onto firm-wide read
    /// endpoints (e.g. case directory). Per the updated Roles policy, Super
    /// Admin does not view case data at all, so this is now just an alias
    /// for AllFirmUsers. Kept as a separate name (rather than deleting) so
    /// existing [Authorize(Roles = RoleNames.AllFirmUsersAndSuperAdmin)]
    /// call sites don't need to change.
    /// </summary>
    public const string AllFirmUsersAndSuperAdmin = AllFirmUsers;

    /// <summary>
    /// Document viewers - Lawyers and authorized Moharrir
    /// </summary>
    public const string CanViewDocuments = Partner + "," + AssociateLawyer + "," + Moharrir;

    /// <summary>
    /// Case creators - Partner and Firm Admin. Super Admin excluded - case
    /// creation is firm business, not a platform-owner task.
    /// </summary>
    public const string CanCreateCases = Partner + "," + FirmAdmin;

    /// <summary>
    /// Case assignment managers - Partner and Firm Admin. Super Admin
    /// excluded - case assignment is firm business, not a platform-owner task.
    /// </summary>
    public const string CanAssignCases = Partner + "," + FirmAdmin;
}