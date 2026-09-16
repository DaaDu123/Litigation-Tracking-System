using MediatR;

namespace LTSBackend.Features.CaseStatuses.Commands.DeleteCaseStatus;

public sealed record DeleteCaseStatusCommand(int StatusID) : IRequest<bool>;
