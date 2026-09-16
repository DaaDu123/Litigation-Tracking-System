using MediatR;

namespace LTSBackend.Features.Cases.Commands.UpdateCaseStatus;

public record UpdateCaseStatusCommand(
    long CaseID,
    int NewStatusID,
    string? Remarks
) : IRequest<bool>;