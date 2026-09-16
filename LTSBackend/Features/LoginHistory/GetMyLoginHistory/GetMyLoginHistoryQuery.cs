using LTSBackend.Features.LoginHistory.DTOs;
using MediatR;
namespace LTSBackend.Features.LoginHistory.GetMyLoginHistory;

public record GetMyLoginHistoryQuery(int UserID) : IRequest<List<MyLoginHistoryDTO>>;