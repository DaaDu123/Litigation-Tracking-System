using LTSFrontend.Core.Http;
using LTSFrontend.Features.Deadlines.DTOs;

namespace LTSFrontend.Features.Deadlines.Services
{
    public class DeadlineService : IDeadlineService
    {
        private readonly ApiClient _api;

        public DeadlineService(ApiClient api)
        {
            _api = api;
        }

        public async Task<List<DeadlineDTO>> GetByCaseAsync(long caseId, bool? completed = null)
        {
            var url = ApiEndpoints.Deadlines.ByCase(caseId);
            if (completed.HasValue) url += $"?completed={completed.Value.ToString().ToLowerInvariant()}";
            var result = await _api.GetAsync<List<DeadlineDTO>>(url);
            return result ?? new List<DeadlineDTO>();
        }

        public async Task<List<DeadlineDTO>> GetUpcomingAsync(int? daysAhead = null)
        {
            var url = ApiEndpoints.Deadlines.Upcoming;
            if (daysAhead.HasValue) url += $"?daysAhead={daysAhead.Value}";
            var result = await _api.GetAsync<List<DeadlineDTO>>(url);
            return result ?? new List<DeadlineDTO>();
        }

        public Task<long> CreateAsync(DeadlineFormDTO form) =>
            _api.PostAsync<long>(ApiEndpoints.Deadlines.Base_, new
            {
                form.CaseID,
                form.DeadlineType,
                DueDate = form.DueDate!.Value,
                form.ReminderDays,
                Remarks = string.IsNullOrWhiteSpace(form.Remarks) ? null : form.Remarks.Trim()
            });

        public Task<bool> UpdateAsync(DeadlineFormDTO form) =>
            _api.PutAsync<bool>(ApiEndpoints.Deadlines.ById(form.DeadlineID), new
            {
                form.DeadlineID,
                form.DeadlineType,
                DueDate = form.DueDate!.Value,
                form.ReminderDays,
                Remarks = string.IsNullOrWhiteSpace(form.Remarks) ? null : form.Remarks.Trim()
            });

        public Task<bool> CompleteAsync(long id) =>
            _api.PutAsync<bool>(ApiEndpoints.Deadlines.Complete(id));

        public Task<bool> DeleteAsync(long id) =>
            _api.DeleteAsync<bool>(ApiEndpoints.Deadlines.ById(id));
    }
}
