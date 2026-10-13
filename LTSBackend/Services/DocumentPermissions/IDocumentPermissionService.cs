using LTSBackend.Comman.Enum;

namespace LTSBackend.Services.DocumentPermissions;





public interface IDocumentPermissionService
{








    Task<bool> CanUserAccessDocumentAsync(int userId, long documentId, string action, CancellationToken cancellationToken = default);










    Task<bool> CanUserUploadToCaseAsync(int userId, long caseId, CancellationToken cancellationToken = default);





    Task<bool> HasMohallirElevatedAccessAsync(int userId, CancellationToken cancellationToken = default);




    Task<bool> IsMohallirRestrictedAsync(int userId, CancellationToken cancellationToken = default);





    Task<DocumentAccessLevel> GetUserDocumentAccessLevelAsync(int userId, CancellationToken cancellationToken = default);




    Task GrantDocumentPermissionAsync(long documentId, int roleId, bool canView, bool canDownload, bool canUpload, CancellationToken cancellationToken = default);





    Task GrantUserDocumentPermissionAsync(long documentId, int userId, bool canView, bool canDownload, bool canUpload, CancellationToken cancellationToken = default);




    Task RevokeDocumentPermissionAsync(long documentId, int roleId, CancellationToken cancellationToken = default);
}