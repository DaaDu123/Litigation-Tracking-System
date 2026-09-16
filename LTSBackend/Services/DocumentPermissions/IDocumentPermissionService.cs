using LTSBackend.Comman.Enum;

namespace LTSBackend.Services.DocumentPermissions;

/// <summary>
/// Manages document access permissions, including the Moharrir blind-upload
/// (write-only) mode required by the SRS.
/// </summary>
public interface IDocumentPermissionService
{
    /// <summary>
    /// Checks whether the user may perform the given action ("View",
    /// "Download", "Upload") on the given document, honouring tenant
    /// isolation, case assignment, and Moharrir restricted/elevated mode.
    /// </summary>
    /// <param name="userId">Acting user ID</param>
    /// <param name="documentId">Target document ID</param>
    /// <param name="action">Action: "View", "Download", "Upload"</param>
    Task<bool> CanUserAccessDocumentAsync(int userId, long documentId, string action, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether the user may upload a NEW document to the given case.
    /// This is intentionally separate from CanUserAccessDocumentAsync: at
    /// upload time no Document row exists yet, so any check that resolves
    /// case assignment "via the document" (as CanUserAccessDocumentAsync's
    /// internal case-assignment lookup does) can never succeed - there is
    /// no document to join through. This method checks assignment directly
    /// against the CaseID instead.
    /// </summary>
    Task<bool> CanUserUploadToCaseAsync(int userId, long caseId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether a Moharrir has been elevated (granted view/download)
    /// rather than being restricted to blind (write-only) upload.
    /// </summary>
    Task<bool> HasMohallirElevatedAccessAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether a Moharrir is in restricted (blind upload only) mode.
    /// </summary>
    Task<bool> IsMohallirRestrictedAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the user's overall document access level (None / WriteOnly /
    /// ReadWrite / FullAccess).
    /// </summary>
    Task<DocumentAccessLevel> GetUserDocumentAccessLevelAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Grants (or updates) a document permission for an entire role.
    /// </summary>
    Task GrantDocumentPermissionAsync(long documentId, int roleId, bool canView, bool canDownload, bool canUpload, CancellationToken cancellationToken = default);

    /// <summary>
    /// Grants (or updates) a document permission for one specific user,
    /// which takes priority over any role-level grant for that same document.
    /// </summary>
    Task GrantUserDocumentPermissionAsync(long documentId, int userId, bool canView, bool canDownload, bool canUpload, CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes a role-level document permission.
    /// </summary>
    Task RevokeDocumentPermissionAsync(long documentId, int roleId, CancellationToken cancellationToken = default);
}