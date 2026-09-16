using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using LTSBackend.Services.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.Notifications.Commands.DeleteNotification;

public class DeleteNotificationHandler(AppDbContext _context, ICurrentUserService _currentUser) : IRequestHandler<DeleteNotificationCommand, bool>
{
    // =====================================================
    // HANDLE — permanently removes one of the caller's own notifications
    // Same IDOR guard as MarkAsReadHandler: a user may only delete their
    // own notifications.
    // =====================================================
    public async Task<bool> Handle(DeleteNotificationCommand request, CancellationToken cancellationToken)
    {
        var notification = await _context.Notifications.FirstOrDefaultAsync(n => n.NotificationID == request.NotificationID, cancellationToken);

        // SECURITY (IDOR): same rationale as MarkAsReadHandler - a user may
        // only dismiss their own notifications.
        if (notification == null || notification.UserID != _currentUser.UserID)
        {
            throw new NotFoundException($"Notification ID {request.NotificationID} not found");
        }

        _context.Notifications.Remove(notification);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
