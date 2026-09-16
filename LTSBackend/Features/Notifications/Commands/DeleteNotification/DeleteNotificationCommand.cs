using MediatR;

namespace LTSBackend.Features.Notifications.Commands.DeleteNotification;

public class DeleteNotificationCommand : IRequest<bool>
{
    public long NotificationID { get; set; }
}
