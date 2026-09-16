using MediatR;

namespace LTSBackend.Features.DocumentTypes.Commands.CreateDocumentType;

public sealed record CreateDocumentTypeCommand(string TypeName, string? Description, bool IsActive = true) : IRequest<int>;
