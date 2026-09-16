using LTSFrontend.Core.DTOs;
using LTSFrontend.Features.Notifications.DTOs;

namespace LTSFrontend.Features.Notifications.Services
{
    public interface INotificationService
    {
        Task<PagedResult<NotificationDTO>> GetMyAsync(bool? isRead = null, int pageNumber = 1, int pageSize = 10);
        Task<int> GetUnreadCountAsync();
        Task<bool> MarkAsReadAsync(long id);
        Task<int> MarkAllAsReadAsync();
        Task<bool> DeleteAsync(long id);
    }
}
