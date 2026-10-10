using MediatR;

namespace LTSBackend.Features.Cases.Commands.DeleteCase;

/// <summary>
/// PERMANENT deletion (hard delete) of an already-archived case. Requires the
/// exact case number as confirmation plus a written reason; FirmAdmin only.
/// </summary>
public record DeleteCaseCommand(long CaseID, string ConfirmCaseNumber, string Reason) : IRequest<bool>;
