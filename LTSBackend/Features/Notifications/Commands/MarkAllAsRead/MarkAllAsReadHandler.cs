using LTSBackend.Data;
using LTSBackend.Services.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.Notifications.Commands.MarkAllAsRead;

public class MarkAllAsReadHandler(AppDbContext _context, ICurrentUserService _currentUser) : IRequestHandler<MarkAllAsReadCommand, int>
{




    public async Task<int> Handle(MarkAllAsReadCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.UserID.HasValue)
        {
            return 0;
        }

        var unread = await _context.Notifications.Where(n => n.UserID == _currentUser.UserID.Value && !n.IsRead).ToListAsync(cancellationToken);

        if (unread.Count == 0)
        {
            return 0;
        }

        var now = DateTime.UtcNow;
        foreach (var n in unread)
        {
            n.IsRead = true;
            n.ReadDate = now;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return unread.Count;
    }
}
