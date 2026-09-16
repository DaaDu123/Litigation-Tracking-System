using MediatR;
namespace LTSBackend.Features.LoginHistory.DeleteLoginHistory;

public record DeleteLoginHistoryCommand(int LoginID) : IRequest<bool>;