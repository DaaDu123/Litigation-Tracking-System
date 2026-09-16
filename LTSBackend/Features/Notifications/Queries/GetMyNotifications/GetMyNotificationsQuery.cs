using LTSBackend.Comman.Responses;
using LTSBackend.Features.Notifications.DTOs;
using MediatR;

namespace LTSBackend.Features.Notifications.Queries.GetMyNotifications;
public record GetMyNotificationsQuery(bool? IsRead,int PageNumber = 1,int PageSize = 10 ) : IRequest<PagedResult<NotificationDTO>>;
