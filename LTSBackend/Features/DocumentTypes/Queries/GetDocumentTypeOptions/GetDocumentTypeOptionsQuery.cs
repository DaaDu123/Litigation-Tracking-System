using LTSBackend.Features.DocumentTypes.DTOs;
using MediatR;

namespace LTSBackend.Features.DocumentTypes.Queries.GetDocumentTypeOptions;

/// <summary>
/// Always active-only, no search - this exists purely to populate the
/// "Document Type" dropdown on the Upload Document form for roles that
/// cannot reach the full master-data GetAll endpoint (AssociateLawyer,
/// Moharrir, InternParalegal). See DocumentTypesController's "options"
/// action and GetDocumentTypeOptionsHandler.
/// </summary>
public sealed record GetDocumentTypeOptionsQuery() : IRequest<List<DocumentTypeOptionDTO>>;
