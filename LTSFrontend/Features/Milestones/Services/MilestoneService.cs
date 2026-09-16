using LTSFrontend.Core.Http;
using LTSFrontend.Features.Milestones.DTOs;

namespace LTSFrontend.Features.Milestones.Services
{
    public class MilestoneService : IMilestoneService
    {
        private readonly ApiClient _api;

        public MilestoneService(ApiClient api)
        {
            _api = api;
        }

        public async Task<List<MilestoneDTO>> GetByCaseAsync(long caseId)
        {
            var result = await _api.GetAsync<List<MilestoneDTO>>(ApiEndpoints.Milestones.ByCase(caseId));
            return result ?? new List<MilestoneDTO>();
        }

        public Task<long> CreateAsync(MilestoneFormDTO form) =>
            _api.PostAsync<long>(ApiEndpoints.Milestones.Base_, new
            {
                form.CaseID,
                form.Milestone,
                MilestoneDate = form.MilestoneDate!.Value,
                Description = string.IsNullOrWhiteSpace(form.Description) ? null : form.Description.Trim()
            });

        public Task<bool> UpdateAsync(MilestoneFormDTO form) =>
            _api.PutAsync<bool>(ApiEndpoints.Milestones.ById(form.MilestoneID), new
            {
                form.MilestoneID,
                form.Milestone,
                MilestoneDate = form.MilestoneDate!.Value,
                Description = string.IsNullOrWhiteSpace(form.Description) ? null : form.Description.Trim()
            });

        public Task<bool> CompleteAsync(long id) =>
            _api.PutAsync<bool>(ApiEndpoints.Milestones.Complete(id));

        public Task<bool> DeleteAsync(long id) =>
            _api.DeleteAsync<bool>(ApiEndpoints.Milestones.ById(id));
    }
}
