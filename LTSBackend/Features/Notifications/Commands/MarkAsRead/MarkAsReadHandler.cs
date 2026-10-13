using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using LTSBackend.Services.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.Notifications.Commands.MarkAsRead;

public class MarkAsReadHandler(AppDbContext _context, ICurrentUserService _currentUser) : IRequestHandler<MarkAsReadCommand, bool>
{






    public async Task<bool> Handle(MarkAsReadCommand request, CancellationToken cancellationToken)
    {
        var notification = await _context.Notifications.FirstOrDefaultAsync(n => n.NotificationID == request.NotificationID, cancellationToken);





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
