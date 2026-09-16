using LTSBackend.Features.DocumentTypes.DTOs;
using MediatR;

namespace LTSBackend.Features.DocumentTypes.Queries.GetAllDocumentTypes;

public sealed record GetAllDocumentTypesQuery(string? SearchText = null, bool ActiveOnly = true) : IRequest<List<DocumentTypeDTO>>;
