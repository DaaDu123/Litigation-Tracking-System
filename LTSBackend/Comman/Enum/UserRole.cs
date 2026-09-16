namespace LTSBackend.Comman.Enum;

public enum UserRole
{
    /// <summary>
    /// System-wide management and data custody
    /// Manages firms, audit logs, data migration
    /// </summary>
    SuperAdmin = 1,

    /// <summary>
    /// Workspace owner - manages specific law firm
    /// Controls users, cases, billing, firm-wide access
    /// </summary>
    FirmAdmin = 2,

    /// <summary>
    /// Senior lawyer - supervises case teams
    /// Assigns lawyers, approves critical filings
    /// </summary>
    Partner = 3,

    /// <summary>
    /// Day-to-day legal work
    /// Drafts pleadings, uploads evidence, tracks deadlines
    /// </summary>
    AssociateLawyer = 4,

    /// <summary>
    /// Legal clerk / Data entry operator
    /// Manages court diaries, handles data entry
    /// Can have conditional permissions (restricted/elevated)
    /// </summary>
    Moharrir = 5,

    /// <summary>
    /// Temporary staff / Junior assistant
    /// Read-only or draft-only permissions
    /// </summary>
    InternParalegal = 6
}