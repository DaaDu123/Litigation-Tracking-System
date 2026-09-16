using MediatR;

namespace LTSBackend.Features.Cases.Commands.DeleteCase;

public record DeleteCaseCommand(long CaseID) : IRequest<bool>;