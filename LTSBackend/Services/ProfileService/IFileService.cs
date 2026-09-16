using Microsoft.AspNetCore.Http;
namespace LTSBackend.Services.ProfileService
{
    public interface IFileService
    {
        /// <summary>
        /// Saves a file under wwwroot/uploads/{folderName} - PUBLICLY reachable
        /// via app.UseStaticFiles() with no authentication. Use ONLY for content
        /// that is meant to be publicly viewable without login, e.g. profile
        /// pictures. Never use this for confidential/tenant-owned content such
        /// as case documents - use SaveSecureFileAsync for those.
        /// </summary>
        Task<string> SaveFileAsync(IFormFile file, string folderName);

        /// <summary>
        /// Deletes a file previously saved via SaveFileAsync (public wwwroot/uploads store).
        /// </summary>
        void DeleteFile(string? relativePath);

        /// <summary>
        /// Reads the raw bytes of a file previously saved via ReadSecureFileAsync's
        /// SecureStorage location (only ever called internally now via
        /// ReadCaseDocumentAsync). Callers must have already performed their own
        /// authorization check (e.g. IDocumentPermissionService.
        /// CanUserAccessDocumentAsync) before calling this - this method itself
        /// does not check permissions, only resolves and reads the file safely.
        /// </summary>
        Task<byte[]> ReadSecureFileAsync(string relativePath);

        /// <summary>
        /// Deletes a file previously saved to the secure (non-web-servable) store.
        /// Only ever called internally now via DeleteCaseDocument.
        /// </summary>
        void DeleteSecureFile(string? relativePath);

        /// <summary>
        /// Saves a case document under the tenant-isolated secure folder hierarchy
        /// Firm/{firmId}/Case/{caseId}/Documents/. Every firm gets its own root
        /// folder, and every case gets its own folder under that firm's folder,
        /// so a case's documents are physically segregated on disk from every
        /// other firm's and every other case's documents - not just filtered at
        /// query time. Returns the relative path to store on the Document row
        /// (e.g. "Firm/10/Case/125/Documents/{guid}.pdf").
        /// </summary>
        Task<string> SaveCaseDocumentAsync(IFormFile file, int firmId, long caseId);

        /// <summary>
        /// Reads a case document's bytes back, but ONLY after verifying the
        /// stored relative path actually lives under the expected
        /// Firm/{firmId}/Case/{caseId}/Documents/ folder for the firmId/caseId
        /// supplied by the caller. This is a second, independent check beyond
        /// EF Core's tenant query filter and the caller's own DocumentPermission
        /// check - it stops a caller from reaching another firm's or another
        /// case's file on disk even if a DocumentID/CaseID were tampered with
        /// upstream. Throws UnauthorizedAccessException on any mismatch.
        /// </summary>
        Task<byte[]> ReadCaseDocumentAsync(string relativePath, int firmId, long caseId);

        /// <summary>
        /// Deletes a case document, applying the same Firm/{firmId}/Case/{caseId}
        /// path-ownership validation as ReadCaseDocumentAsync before touching disk.
        /// </summary>
        void DeleteCaseDocument(string? relativePath, int firmId, long caseId);
    }
}