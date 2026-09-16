using LTSFrontend.Core.Http;
using LTSFrontend.Features.CaseParties.DTOs;

namespace LTSFrontend.Features.CaseParties.Services
{
    public class CasePartyService : ICasePartyService
    {
        private readonly ApiClient _api;

        public CasePartyService(ApiClient api)
        {
            _api = api;
        }

        public async Task<List<CasePartyDTO>> GetByCaseAsync(long caseId)
        {
            var result = await _api.GetAsync<List<CasePartyDTO>>(ApiEndpoints.CaseParties.ByCase(caseId));
            return result ?? new List<CasePartyDTO>();
        }

        public Task<long> CreateAsync(CasePartyFormDTO dto) =>
            _api.PostAsync<long>(ApiEndpoints.CaseParties.ByCase(dto.CaseID), new
            {
                dto.PartyType,
                PartyName = dto.PartyName.Trim(),
                Organization = Norm(dto.Organization),
                CNIC = Norm(dto.CNIC),
                NTN = Norm(dto.NTN),
                ContactNo = Norm(dto.ContactNo),
                Email = Norm(dto.Email),
                Address = Norm(dto.Address),
                LawyerName = Norm(dto.LawyerName),
                Remarks = Norm(dto.Remarks)
            });

        public Task<bool> UpdateAsync(CasePartyFormDTO dto) =>
            _api.PutAsync<bool>(ApiEndpoints.CaseParties.ById(dto.CaseID, dto.PartyID), new
            {
                dto.PartyType,
                PartyName = dto.PartyName.Trim(),
                Organization = Norm(dto.Organization),
                CNIC = Norm(dto.CNIC),
                NTN = Norm(dto.NTN),
                ContactNo = Norm(dto.ContactNo),
                Email = Norm(dto.Email),
                Address = Norm(dto.Address),
                LawyerName = Norm(dto.LawyerName),
                Remarks = Norm(dto.Remarks)
            });

        public Task<bool> DeleteAsync(long caseId, long partyId) =>
            _api.DeleteAsync<bool>(ApiEndpoints.CaseParties.ById(caseId, partyId));

        private static string? Norm(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();
    }
}
