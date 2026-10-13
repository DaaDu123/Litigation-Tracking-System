using MediatR;

namespace LTSBackend.Features.Cases.Commands.RestoreCase;


public record RestoreCaseCommand(long CaseID) : IRequest<bool>;
