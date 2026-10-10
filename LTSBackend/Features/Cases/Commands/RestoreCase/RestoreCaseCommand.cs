using MediatR;

namespace LTSBackend.Features.Cases.Commands.RestoreCase;

/// <summary>Un-archives a soft-deleted case.</summary>
public record RestoreCaseCommand(long CaseID) : IRequest<bool>;
