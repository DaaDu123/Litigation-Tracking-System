using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using LTSBackend.Services.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.Notifications.Commands.MarkAsRead;

public class MarkAsReadHandler(AppDbContext _context, ICurrentUserService _currentUser) : IRequestHandler<MarkAsReadCommand, bool>
{
    // =====================================================
    // HANDLE — marks a single notification as read
    // IDOR guard: only the notification's own addressee can mark it read
    // — 404, not 403, so a foreign NotificationID's existence isn't
    // disclosed.
    // =====================================================
    public async Task<bool> Handle(MarkAsReadCommand request, CancellationToken cancellationToken)
    {
        var notification = await _context.Notifications.FirstOrDefaultAsync(n => n.NotificationID == request.NotificationID, cancellationToken);

        // SECURITY (IDOR): a notification can only ever be marked read by
        // the user it was addressed to - never trust the route param alone.
        // 404 (not 403) is intentional so we don't leak whether a given
        // NotificationID belongs to someone else.
        if (notification == null || notification.UserID != _currentUser.UserID)
        {
            throw new NotFoundException($"Notification ID {request.NotificationID} not found");
        }

        if (!notification.IsRead)
        {
            notification.IsRead = true;
            notification.ReadDate = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }
        return true;
    }
}
