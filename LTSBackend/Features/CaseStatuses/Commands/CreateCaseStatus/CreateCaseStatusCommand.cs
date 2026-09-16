using MediatR;

namespace LTSBackend.Features.CaseStatuses.Commands.CreateCaseStatus;

public sealed record CreateCaseStatusCommand(string StatusName, int SequenceNo, string ColorCode, bool IsClosed, bool IsActive = true) : IRequest<int>;
