using MediatR;

namespace LTSBackend.Features.Cases.Commands.ArchiveCase;


public record ArchiveCaseCommand(long CaseID, string? Reason) : IRequest<bool>;
