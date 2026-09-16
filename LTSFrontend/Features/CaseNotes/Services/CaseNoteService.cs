using LTSFrontend.Core.Http;
using LTSFrontend.Features.CaseNotes.DTOs;

namespace LTSFrontend.Features.CaseNotes.Services
{
    public class CaseNoteService : ICaseNoteService
    {
        private readonly ApiClient _api;

        public CaseNoteService(ApiClient api)
        {
            _api = api;
        }

        public async Task<List<CaseNoteDTO>> GetByCaseAsync(long caseId)
        {
            var result = await _api.GetAsync<List<CaseNoteDTO>>(ApiEndpoints.CaseNotes.ByCase(caseId));
            return result ?? new List<CaseNoteDTO>();
        }

        public Task<long> CreateAsync(CaseNoteFormDTO dto) =>
            _api.PostAsync<long>(ApiEndpoints.CaseNotes.Base_, new
            {
                dto.CaseID,
                dto.NoteType,
                Notes = dto.Notes.Trim()
            });

        public Task<bool> UpdateAsync(CaseNoteFormDTO dto) =>
            _api.PutAsync<bool>(ApiEndpoints.CaseNotes.ById(dto.NoteID), new
            {
                dto.NoteID,
                dto.NoteType,
                Notes = dto.Notes.Trim()
            });

        public Task<bool> DeleteAsync(long noteId) =>
            _api.DeleteAsync<bool>(ApiEndpoints.CaseNotes.ById(noteId));
    }
}
