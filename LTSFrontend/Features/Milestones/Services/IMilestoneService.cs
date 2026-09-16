using LTSFrontend.Features.Milestones.DTOs;

namespace LTSFrontend.Features.Milestones.Services
{
    public interface IMilestoneService
    {
        Task<List<MilestoneDTO>> GetByCaseAsync(long caseId);
        Task<long> CreateAsync(MilestoneFormDTO form);
        Task<bool> UpdateAsync(MilestoneFormDTO form);
        Task<bool> CompleteAsync(long id);
        Task<bool> DeleteAsync(long id);
    }
}
