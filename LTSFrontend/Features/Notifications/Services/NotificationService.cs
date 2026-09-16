using LTSFrontend.Core.Http;
using LTSFrontend.Core.DTOs;
using LTSFrontend.Features.Notifications.DTOs;

namespace LTSFrontend.Features.Notifications.Services
{
    public class NotificationService (ApiClient _api) : INotificationService
    {
        public async Task<PagedResult<NotificationDTO>> GetMyAsync(bool? isRead = null, int pageNumber = 1, int pageSize = 10)
        {
            var query = new List<string>
            {
                $"pageNumber={pageNumber}", $"pageSize={pageSize}"
            };
            if (isRead.HasValue) query.Add($"isRead={isRead.Value.ToString().ToLowerInvariant()}");

            var url = ApiEndpoints.Notifications.Base_ + "?" + string.Join("&", query);
            var result = await _api.GetAsync<PagedResult<NotificationDTO>>(url);
            return result ?? new PagedResult<NotificationDTO> { PageNumber = pageNumber, PageSize = pageSize };
        }
        public async Task<int> GetUnreadCountAsync()
        {
            var result = await _api.GetAsync<UnreadCountDTO>(ApiEndpoints.Notifications.UnreadCount);
            return result?.UnreadCount ?? 0;
        }

        public Task<bool> MarkAsReadAsync(long id)
        {
            return _api.PutAsync<bool>(ApiEndpoints.Notifications.MarkAsRead(id));
        }

        public Task<int> MarkAllAsReadAsync()
        {
            return _api.PutAsync<int>(ApiEndpoints.Notifications.MarkAllAsRead);
        }

        public Task<bool> DeleteAsync(long id)
        {
            return _api.DeleteAsync<bool>(ApiEndpoints.Notifications.ById(id));
        }
    }
}
