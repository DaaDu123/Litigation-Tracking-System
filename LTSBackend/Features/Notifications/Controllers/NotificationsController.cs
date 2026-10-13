using LTSBackend.Comman.Responses;
using LTSBackend.Features.Notifications.Commands.DeleteNotification;
using LTSBackend.Features.Notifications.Commands.MarkAllAsRead;
using LTSBackend.Features.Notifications.Commands.MarkAsRead;
using LTSBackend.Features.Notifications.DTOs;
using LTSBackend.Features.Notifications.Queries.GetMyNotifications;
using LTSBackend.Features.Notifications.Queries.GetUnreadCount;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LTSBackend.Features.Notifications.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class NotificationsController(IMediator _mediator) : ControllerBase
{






    [HttpGet]
    public async Task<IActionResult> GetMy([FromQuery] bool? isRead, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _mediator.Send(new GetMyNotificationsQuery(isRead, pageNumber, pageSize));
        return Ok(ApiResponse<PagedResult<NotificationDTO>>.SuccessResponse(result, "Notifications fetched successfully."));
    }






    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        var result = await _mediator.Send(new GetUnreadCountQuery());
        return Ok(ApiResponse<UnreadCountDTO>.SuccessResponse(result, "Unread count fetched successfully."));
    }





    [HttpPut("{id}/read")]
    public async Task<IActionResult> MarkAsRead(long id)
    {
        var result = await _mediator.Send(new MarkAsReadCommand { NotificationID = id });
        return Ok(ApiResponse<bool>.SuccessResponse(result, "Notification marked as read."));
    }






    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var count = await _mediator.Send(new MarkAllAsReadCommand());
        return Ok(ApiResponse<int>.SuccessResponse(count, $"{count} notification(s) marked as read."));
    }





    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        var result = await _mediator.Send(new DeleteNotificationCommand { NotificationID = id });
        return Ok(ApiResponse<bool>.SuccessResponse(result, "Notification deleted."));
    }
}
