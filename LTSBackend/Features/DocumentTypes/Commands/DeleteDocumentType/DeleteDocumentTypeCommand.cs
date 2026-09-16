using MediatR;

namespace LTSBackend.Features.DocumentTypes.Commands.DeleteDocumentType;

public sealed record DeleteDocumentTypeCommand(int DocumentTypeID) : IRequest<bool>;
