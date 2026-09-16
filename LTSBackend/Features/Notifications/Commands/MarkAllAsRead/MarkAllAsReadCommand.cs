using MediatR;

namespace LTSBackend.Features.Notifications.Commands.MarkAllAsRead;

/// <summary>
/// Marks every unread notification belonging to the CURRENT user as read.
/// No parameters - UserID is always resolved server-side, so there is no
/// way to accidentally (or maliciously) mark another user's notifications.
/// </summary>
public class MarkAllAsReadCommand : IRequest<int>
{
}
