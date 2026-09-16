using LTSFrontend.Features.CaseParties.DTOs;

namespace LTSFrontend.Features.CaseParties.Services
{
    public interface ICasePartyService
    {
        Task<List<CasePartyDTO>> GetByCaseAsync(long caseId);
        Task<long> CreateAsync(CasePartyFormDTO dto);
        Task<bool> UpdateAsync(CasePartyFormDTO dto);
        Task<bool> DeleteAsync(long caseId, long partyId);
    }
}
