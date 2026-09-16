using LTSBackend.Comman.Enum;
using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using LTSBackend.Features.Documents.DTOs;
using LTSBackend.Services.DocumentPermissions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.Documents.Queries.GetDocument;

public class GetDocumentHandler(AppDbContext _context, IDocumentPermissionService _permissionService, ILogger<GetDocumentHandler> _logger) : IRequestHandler<GetDocumentQuery, DocumentDetailDTO?>
{
    // =====================================================
    // HANDLE — checks view permission and returns a document's metadata
    // Calls CanUserAccessDocumentAsync (View), then returns the
    // document's metadata only — file bytes are served separately via
    // DownloadDocumentHandler.
    // =====================================================
    public async Task<DocumentDetailDTO?> Handle(GetDocumentQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Get document request - ID: {DocumentId}, User: {UserId}", request.DocumentID, request.UserID);

        // 1. Check user permissions
        bool canView = await _permissionService.CanUserAccessDocumentAsync(request.UserID,request.DocumentID,"View",cancellationToken);

        if (!canView)
        {
            _logger.LogWarning("User {UserId} denied access to document {DocumentId}", request.UserID, request.DocumentID);
            throw new UnauthorizedException("You don't have permission to view this document");
        }

        // 2. Fetch document
        var document = await _context.Documents
            .AsNoTracking()
            .Include(x => x.DocumentType)
            .Include(x => x.Case)
            .FirstOrDefaultAsync(x => x.DocumentID == request.DocumentID, cancellationToken);

        if (document == null)
        {
            _logger.LogWarning("Document not found: {DocumentId}", request.DocumentID);
            throw new NotFoundException($"Document {request.DocumentID} not found");
        }

        // 3. Map to DTO
        var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(x => x.UserID == document.UploadedBy, cancellationToken);

        string? approvedByName = null;
        if (document.ApprovedBy.HasValue)
        {
            approvedByName = await _context.Users
                .AsNoTracking()
                .Where(x => x.UserID == document.ApprovedBy.Value)
                .Select(x => x.FullName)
                .FirstOrDefaultAsync(cancellationToken);
        }

        var documentDetail = new DocumentDetailDTO
        {
            DocumentID = document.DocumentID,
            CaseID = document.CaseID,
            CaseNumber = document.Case?.CaseNumber ?? "Unknown",
            DocumentName = document.DocumentName,
            FileName = document.FileName,
            DocumentType = document.DocumentType?.TypeName ?? "Unknown",
            FileSize = document.FileSize,
            VersionNo = document.VersionNo,
            IsLatest = document.IsLatest,
            UploadedBy = user?.FullName ?? "Unknown User",
            UploadedDate = document.UploadedDate,
            Remarks = document.Remarks ?? string.Empty,
            IsDraft = document.IsDraft,
            ApprovedByName = approvedByName,
            ApprovedDate = document.ApprovedDate
        };

        _logger.LogInformation("Document retrieved successfully: {DocumentId}", request.DocumentID);

        return documentDetail;
    }
}