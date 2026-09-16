using MediatR;
namespace LTSBackend.Features.LoginHistory.DeleteAllOldHistory;

public record DeleteOldHistoryCommand(int Days) : IRequest<int>;