namespace LTSFrontend.Core.Enums
{
    /// <summary>
    /// Mirrors LTSBackend.Comman.Enum.DocumentAccessLevel - the effective
    /// access a given user has to a given document (or document type),
    /// combining their role's baseline rights with any case-level or
    /// document-level overrides. Used purely for UI decisions (which
    /// buttons/icons to show); the backend enforces the real boundary on
    /// every document endpoint independently.
    /// </summary>
    public enum DocumentAccessLevel
    {
        /// <summary>No access at all - document is hidden from this user.</summary>
        None = 0,

        /// <summary>Can upload new versions, but cannot view or download existing ones.</summary>
        WriteOnly = 1,

        /// <summary>Can view the document (e.g. inline preview), but not download it.</summary>
        ReadOnly = 2,

        /// <summary>Can both view and download the document.</summary>
        ReadWrite = 3,

        /// <summary>Full access: view, download, and upload new versions.</summary>
        FullAccess = 4
    }

    /// <summary>Display/UI helpers for <see cref="DocumentAccessLevel"/>.</summary>
    public static class DocumentAccessLevelExtensions
    {
        /// <summary>True if this access level permits opening/previewing the document.</summary>
        public static bool CanView(this DocumentAccessLevel level)
        {
            return level is DocumentAccessLevel.ReadOnly or DocumentAccessLevel.ReadWrite or DocumentAccessLevel.FullAccess;
        }

        /// <summary>True if this access level permits downloading the document.</summary>
        public static bool CanDownload(this DocumentAccessLevel level)
        {
            return level is DocumentAccessLevel.ReadWrite or DocumentAccessLevel.FullAccess;
        }

        /// <summary>True if this access level permits uploading a new version.</summary>
        public static bool CanUpload(this DocumentAccessLevel level)
        {
            return level is DocumentAccessLevel.WriteOnly or DocumentAccessLevel.FullAccess;
        }

        /// <summary>Badge label used on Documents list/detail screens.</summary>
        public static string ToDisplayText(this DocumentAccessLevel level)
        {
            return level switch
            {
                DocumentAccessLevel.None => "No Access",
                DocumentAccessLevel.WriteOnly => "Upload Only",
                DocumentAccessLevel.ReadOnly => "View Only",
                DocumentAccessLevel.ReadWrite => "View & Download",
                DocumentAccessLevel.FullAccess => "Full Access",
                _ => "Unknown"
            };
        }
    }
}
