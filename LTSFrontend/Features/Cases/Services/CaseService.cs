using LTSFrontend.Core.Http;
using LTSFrontend.Core.DTOs;
using LTSFrontend.Features.Cases.DTOs;

namespace LTSFrontend.Features.Cases.Services
{
    public class CaseService : ICaseService
    {
        private readonly ApiClient _api;

        public CaseService(ApiClient api)
        {
            _api = api;
        }

        public async Task<PagedResult<CaseDTO>> GetAllAsync(
            string? searchText = null,
            int? courtID = null,
            int? statusID = null,
            string? priority = null,
            int pageNumber = 1,
            int pageSize = 10)
        {
            var query = new List<string>
            {
                $"pageNumber={pageNumber}",
                $"pageSize={pageSize}"
            };

            if (!string.IsNullOrWhiteSpace(searchText))
                query.Add($"searchText={Uri.EscapeDataString(searchText)}");
            if (courtID.HasValue && courtID.Value > 0)
                query.Add($"courtID={courtID.Value}");
            if (statusID.HasValue && statusID.Value > 0)
                query.Add($"statusID={statusID.Value}");
            if (!string.IsNullOrWhiteSpace(priority))
                query.Add($"priority={Uri.EscapeDataString(priority)}");

            var url = ApiEndpoints.Cases.Base_ + "?" + string.Join("&", query);
            var result = await _api.GetAsync<PagedResult<CaseDTO>>(url);
            return result ?? new PagedResult<CaseDTO> { PageNumber = pageNumber, PageSize = pageSize };
        }

        public Task<CaseDTO?> GetByIdAsync(long id) =>
            _api.GetAsync<CaseDTO>(ApiEndpoints.Cases.ById(id));

        public Task<long> CreateAsync(CreateCaseDTO form) =>
            _api.PostAsync<long>(ApiEndpoints.Cases.Base_, new
            {
                CaseNumber = form.CaseNumber.Trim(),
                CaseTitle = form.CaseTitle.Trim(),
                CaseDescription = string.IsNullOrWhiteSpace(form.CaseDescription) ? null : form.CaseDescription.Trim(),
                form.CourtID,
                CourtName = string.IsNullOrWhiteSpace(form.CourtName) ? null : form.CourtName.Trim(),
                form.CategoryID,
                CategoryName = string.IsNullOrWhiteSpace(form.CategoryName) ? null : form.CategoryName.Trim(),
                form.Priority,
                SubjectMatter = form.SubjectMatter.Trim(),
                FilingDate = form.FilingDate!.Value,
                InstitutionDate = form.InstitutionDate!.Value,
                RegistrationDate = form.RegistrationDate!.Value,
                form.ExpectedDisposalDate,
                form.ClaimedAmount,
                form.PotentialLiability,
                FinancialImplication = string.IsNullOrWhiteSpace(form.FinancialImplication) ? null : form.FinancialImplication.Trim(),
                form.ResponsibleDepartmentID,
                DepartmentName = string.IsNullOrWhiteSpace(form.DepartmentName) ? null : form.DepartmentName.Trim(),
                form.CurrentLegalOfficerID
            });

        public Task<bool> UpdateAsync(UpdateCaseDTO form)
        {
            return _api.PutAsync<bool>(ApiEndpoints.Cases.ById(form.CaseID), form);
        }

        public Task<bool> DeleteAsync(long id)
        {
            return _api.DeleteAsync<bool>(ApiEndpoints.Cases.ById(id));
        }

        public Task<bool> UpdateStatusAsync(long id, int newStatusID, string? remarks)
        {
            return _api.PutAsync<bool>(ApiEndpoints.Cases.Status(id), new { NewStatusID = newStatusID, Remarks = remarks });
        }

        public Task<bool> UpdateStageAsync(long id, int newStageID)
        {
            return _api.PutAsync<bool>(ApiEndpoints.Cases.ById(id), new UpdateCaseDTO
            {
                CaseID = id,
                StageID = newStageID
            });
        }

        public async Task<List<CaseStatusHistoryDTO>> GetStatusHistoryAsync(long id)
        {
            var result = await _api.GetAsync<List<CaseStatusHistoryDTO>>(ApiEndpoints.Cases.StatusHistory(id));
            return result ?? new List<CaseStatusHistoryDTO>();
        }
    }
}
