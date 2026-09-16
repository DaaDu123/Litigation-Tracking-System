using LTSBackend.Features.Notifications.DTOs;
using MediatR;

namespace LTSBackend.Features.Notifications.Queries.GetUnreadCount;
public record GetUnreadCountQuery : IRequest<UnreadCountDTO>;
