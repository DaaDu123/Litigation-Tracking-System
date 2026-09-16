using LTSBackend.Features.DocumentTypes.DTOs;
using MediatR;

namespace LTSBackend.Features.DocumentTypes.Queries.GetDocumentTypeById;

public sealed record GetDocumentTypeByIdQuery(int DocumentTypeID) : IRequest<DocumentTypeDTO>;
