using MediatR;

namespace LTSBackend.Features.Notifications.Commands.MarkAsRead;

public class MarkAsReadCommand : IRequest<bool>
{
    public long NotificationID { get; set; }
}
