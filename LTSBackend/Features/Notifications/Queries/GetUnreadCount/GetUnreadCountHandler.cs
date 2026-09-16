using LTSBackend.Data;
using LTSBackend.Features.Notifications.DTOs;
using LTSBackend.Services.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.Notifications.Queries.GetUnreadCount;

public class GetUnreadCountHandler(AppDbContext _context, ICurrentUserService _currentUser) : IRequestHandler<GetUnreadCountQuery, UnreadCountDTO>
{
    // =====================================================
    // HANDLE — returns just the caller's unread-notification count
    // Used for the notification-bell badge.
    // =====================================================
    public async Task<UnreadCountDTO> Handle(GetUnreadCountQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.UserID.HasValue)
        {
            return new UnreadCountDTO { UnreadCount = 0 };
        }

        var count = await _context.Notifications
            .AsNoTracking()
            .Where(n => n.UserID == _currentUser.UserID.Value && !n.IsRead)
            .CountAsync(cancellationToken);
        return new UnreadCountDTO { UnreadCount = count };
    }
}
