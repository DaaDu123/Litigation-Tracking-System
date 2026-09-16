using LTSFrontend.Features.Deadlines.DTOs;

namespace LTSFrontend.Features.Deadlines.Services
{
    public interface IDeadlineService
    {
        Task<List<DeadlineDTO>> GetByCaseAsync(long caseId, bool? completed = null);
        Task<List<DeadlineDTO>> GetUpcomingAsync(int? daysAhead = null);
        Task<long> CreateAsync(DeadlineFormDTO form);
        Task<bool> UpdateAsync(DeadlineFormDTO form);
        Task<bool> CompleteAsync(long id);
        Task<bool> DeleteAsync(long id);
    }
}
