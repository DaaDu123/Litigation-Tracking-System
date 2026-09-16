using MediatR;

namespace LTSBackend.Features.CaseStatuses.Commands.UpdateCaseStatus;

public sealed record UpdateCaseStatusCommand(int StatusID, string StatusName, int SequenceNo, string ColorCode, bool IsClosed, bool IsActive = true) : IRequest<bool>;
