using LTSFrontend.Core.Http;
using LTSFrontend.Features.CaseAssignments.DTOs;

namespace LTSFrontend.Features.CaseAssignments.Services
{
    public class CaseAssignmentService : ICaseAssignmentService
    {
        private readonly ApiClient _api;

        public CaseAssignmentService(ApiClient api)
        {
            _api = api;
        }

        public async Task<List<CaseAssignmentDTO>> GetByCaseAsync(long caseId, bool activeOnly = false)
        {
            var url = ApiEndpoints.CaseAssignments.ByCase(caseId) + (activeOnly ? "?activeOnly=true" : "");
            var result = await _api.GetAsync<List<CaseAssignmentDTO>>(url);
            return result ?? new List<CaseAssignmentDTO>();
        }

        public async Task<List<CaseAssignmentDTO>> GetMyAssignedCasesAsync()
        {
            var result = await _api.GetAsync<List<CaseAssignmentDTO>>(ApiEndpoints.CaseAssignments.MyCases);
            return result ?? new List<CaseAssignmentDTO>();
        }

        public Task<long> AssignAsync(AssignCaseDTO dto) =>
            _api.PostAsync<long>(ApiEndpoints.CaseAssignments.Base_, new
            {
                dto.CaseID,
                UserID = dto.UserID!.Value,
                dto.AssignmentType,
                dto.LeadCounsel,
                Remarks = string.IsNullOrWhiteSpace(dto.Remarks) ? null : dto.Remarks.Trim()
            });

        public Task<bool> UpdateAsync(UpdateAssignmentDTO dto) =>
            _api.PutAsync<bool>(ApiEndpoints.CaseAssignments.ById(dto.AssignmentID), new
            {
                dto.AssignmentID,
                dto.AssignmentType,
                dto.LeadCounsel,
                Remarks = string.IsNullOrWhiteSpace(dto.Remarks) ? null : dto.Remarks.Trim()
            });

        // Backend binds this as [FromBody] string? — send the raw string, not an object.
        public Task<bool> EndAsync(long assignmentId, string? remarks) =>
            _api.PutAsync<bool>(ApiEndpoints.CaseAssignments.End(assignmentId), remarks);
    }
}
