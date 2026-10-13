using LTSBackend.Comman.Enum;

namespace LTSBackend.Services.Permissions;

public interface IPermissionService
{




    Task<bool> HasPermissionAsync(int userId, string permission, CancellationToken cancellationToken = default);





    Task<List<string>> GetPermissionsAsync(int userId, CancellationToken cancellationToken = default);




    Task<bool> HasRoleAsync(int userId, UserRole role, CancellationToken cancellationToken = default);





    Task<UserRole?> GetUserRoleAsync(int userId, CancellationToken cancellationToken = default);








    Task<bool> HasFullCaseDirectoryVisibilityAsync(int userId, CancellationToken cancellationToken = default);










    Task<bool> IsUserAssignedToCaseAsync(int userId, long caseId, CancellationToken cancellationToken = default);
}