using LTSBackend.Comman.Responses;
using LTSBackend.Data;
using LTSBackend.Features.Notifications.DTOs;
using LTSBackend.Services.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.Notifications.Queries.GetMyNotifications;

public class GetMyNotificationsHandler (AppDbContext _context, ICurrentUserService _currentUser, ILogger<GetMyNotificationsHandler> _logger) : IRequestHandler<GetMyNotificationsQuery, PagedResult<NotificationDTO>>
{
    // =====================================================
    // HANDLE — paged list of the caller's own notifications
    // Always scoped to the caller's own UserID; optionally filtered to
    // only read or only unread. Returns an empty page if the user's
    // identity can't be resolved.
    // =====================================================
    public async Task<PagedResult<NotificationDTO>> Handle(GetMyNotificationsQuery request,CancellationToken cancellationToken)
    {
        if (!_currentUser.UserID.HasValue)
        {
            return new PagedResult<NotificationDTO>
            {
                Items = [],
                TotalRecords = 0,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }

        var userId = _currentUser.UserID.Value;

        _logger.LogInformation("Fetching notifications for UserID {UserID} - Page: {PageNumber}, Size: {PageSize}",userId, request.PageNumber, request.PageSize);
        var query = _context.Notifications
            .AsNoTracking()
            .Include(n => n.NotificationType)
            .Include(n => n.Case)
            .Where(n => n.UserID == userId)
            .AsQueryable();

        if (request.IsRead.HasValue)
        {
            query = query.Where(n => n.IsRead == request.IsRead.Value);
        }

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(n => n.CreatedDate)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(n => new NotificationDTO
            {
                NotificationID = n.NotificationID,
                NotificationTypeID = n.NotificationTypeID,
                NotificationTypeName = n.NotificationType != null ? n.NotificationType.TypeName : string.Empty,
                CaseID = n.CaseID,
                CaseTitle = n.Case != null ? n.Case.CaseTitle : null,
                CaseNumber = n.Case != null ? n.Case.CaseNumber : null,
                Subject = n.Subject,
                Message = n.Message,
                IsRead = n.IsRead,
                ReadDate = n.ReadDate,
                Priority = n.Priority,
                CreatedDate = n.CreatedDate
            })
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Fetched {Count} notifications of {Total} total for UserID {UserID}",items.Count, total, userId);
        return new PagedResult<NotificationDTO>
        {
            Items = items,
            TotalRecords = total,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}
