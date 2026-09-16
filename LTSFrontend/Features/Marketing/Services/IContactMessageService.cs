using LTSFrontend.Features.Marketing.DTOs;

namespace LTSFrontend.Features.Marketing.Services
{
    public interface IContactMessageService
    {
        Task SubmitAsync(SubmitContactMessage request);
    }
}
