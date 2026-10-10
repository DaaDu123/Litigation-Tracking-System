using LTSFrontend.Core.DTOs;
using LTSFrontend.Features.Cases.DTOs;

namespace LTSFrontend.Features.Cases.Services
{
    public interface ICaseService
    {
        Task<PagedResult<CaseDTO>> GetAllAsync(string? searchText = null, int? courtID = null, int? statusID = null, string? priority = null, int pageNumber = 1, int pageSize = 10, bool archivedOnly = false);

        Task<CaseDTO?> GetByIdAsync(long id);
        Task<CreateCaseResultDTO> CreateAsync(CreateCaseDTO form);
        Task<bool> UpdateAsync(UpdateCaseDTO form);
        /// <summary>Normal "delete" = archive (soft delete). Reversible via RestoreAsync.</summary>
        Task<bool> ArchiveAsync(long id, string? reason = null);
        Task<bool> RestoreAsync(long id);

        /// <summary>Irreversible. Archived cases only, FirmAdmin only; needs the typed case number and a reason.</summary>
        Task<bool> PermanentDeleteAsync(long id, string confirmCaseNumber, string reason);

        /// <summary>Backfill a workflow + checklist for a legacy case.</summary>
        Task<bool> GenerateWorkflowAsync(long id);
        Task<bool> UpdateStatusAsync(long id, int newStatusID, string? remarks);
        Task<bool> UpdateStageAsync(long id, int newStageID);
        Task<List<CaseStatusHistoryDTO>> GetStatusHistoryAsync(long id);

        /// <summary>The case's own stage progress + document checklist (null/empty for cases created before workflow templates existed).</summary>
        Task<CaseWorkflowDTO?> GetWorkflowAsync(long id);
    }
}
