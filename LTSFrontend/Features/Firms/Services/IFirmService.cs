using LTSFrontend.Features.Firms.DTOs;

namespace LTSFrontend.Features.Firms.Services
{
    public interface IFirmService
    {
        Task<List<FirmDTO>> GetAllAsync();
        Task<FirmDTO?> GetByIdAsync(int id);
        Task<int> CreateAsync(CreateFirmDTO dto);
        Task<bool> UpdateAsync(UpdateFirmDTO dto);
        Task<bool> BlockAsync(int id, string? reason);
        Task<bool> UnblockAsync(int id);
        Task<bool> DeleteAsync(int id);
        Task ExportAsync(int id);
    }
}
