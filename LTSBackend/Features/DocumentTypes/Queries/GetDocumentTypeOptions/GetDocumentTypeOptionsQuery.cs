using LTSBackend.Features.DocumentTypes.DTOs;
using MediatR;

namespace LTSBackend.Features.DocumentTypes.Queries.GetDocumentTypeOptions;








public sealed record GetDocumentTypeOptionsQuery() : IRequest<List<DocumentTypeOptionDTO>>;
