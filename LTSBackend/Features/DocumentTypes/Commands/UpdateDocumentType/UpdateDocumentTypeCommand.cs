using MediatR;

namespace LTSBackend.Features.DocumentTypes.Commands.UpdateDocumentType;

public sealed record UpdateDocumentTypeCommand(int DocumentTypeID, string TypeName, string? Description, bool IsActive = true) : IRequest<bool>;
