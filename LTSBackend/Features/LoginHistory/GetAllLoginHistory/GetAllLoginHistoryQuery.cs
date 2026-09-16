using LTSBackend.Comman.Responses;
using LTSBackend.Features.LoginHistory.DTOs;
using MediatR;

namespace LTSBackend.Features.LoginHistory.GetAllLoginHistory;

public record GetAllLoginHistoryQuery(string? Search, DateTime? FromDate, DateTime? ToDate, string? Status,
    int PageNumber = 1, int PageSize = 10) : IRequest<PagedResult<LoginHistoryDTO>>;