using MediatR;

namespace LTSBackend.Features.Cases.Commands.ArchiveCase;

/// <summary>Soft delete: hides the case from normal lists but keeps every record and file. Reversible via RestoreCase.</summary>
public record ArchiveCaseCommand(long CaseID, string? Reason) : IRequest<bool>;
