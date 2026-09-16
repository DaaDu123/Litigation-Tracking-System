using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using LTSBackend.Services.Audit;
using LTSBackend.Services.CurrentUser;
using LTSBackend.Services.ProfileService;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.Documents.Commands.DeleteDocument;

public class DeleteDocumentHandler(AppDbContext _context, IFileService _fileService, IAuditService _auditService,ICurrentUserService _currentUser, ILogger<DeleteDocumentHandler> _logger) : IRequestHandler<DeleteDocumentCommand, bool>
{
    // =====================================================
    // HANDLE — hard-deletes a document, its permissions, and its file
    // Removes DocumentPermissions rows first (FK-safe order), then the DB
    // row, then the file on secure disk storage — a failed file delete is
    // logged but doesn't fail the request, since the DB record is already
    // gone. Writes an audit log entry. Not reversible.
    // =====================================================
    public async Task<bool> Handle(DeleteDocumentCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Document delete attempt - ID: {DocumentId}, User: {UserId}", request.DocumentID, request.UserID);

        // Include Case so we have its FirmID for the storage-layer ownership check below.
        // (Document itself is also tenant-filtered by EF Core's HasQueryFilter on
        // Case.FirmID == RequestFirmId, so this Include also re-confirms that filter fired.)
        var document = await _context.Documents.Include(x => x.Case).FirstOrDefaultAsync(x => x.DocumentID == request.DocumentID, cancellationToken);

        if (document == null)
        {
            _logger.LogWarning("Delete failed: Document not found {DocumentId}", request.DocumentID);
            throw new NotFoundException($"Document {request.DocumentID} not found");
        }

        // Defense-in-depth: re-confirm the document's own case belongs to the
        // caller's firm even though the EF query filter already enforces this,
        // before we ever remove the row or the file on disk. SuperAdmin is
        // platform-level (FirmID is null on their own account) and is
        // intentionally exempt, same as the EF Core tenant query filter's
        // BypassTenantFilter rule.
        if (document.Case == null || (!_currentUser.IsSuperAdmin && document.Case.FirmID != _currentUser.FirmID))
        {
            _logger.LogWarning("Delete denied: document {DocumentId} does not belong to the caller's firm", request.DocumentID);
            throw new NotFoundException($"Document {request.DocumentID} not found");
        }

        int documentFirmId = document.Case.FirmID;
        long documentCaseId = document.CaseID;

        var permissions = await _context.DocumentPermissions.Where(x => x.DocumentID == request.DocumentID).ToListAsync(cancellationToken);
        if (permissions.Any())
        {
            _context.DocumentPermissions.RemoveRange(permissions);
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Removed {Count} permissions for document {DocumentId}", permissions.Count, request.DocumentID);
        }

        _context.Documents.Remove(document);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Document deleted successfully: {DocumentId}", request.DocumentID);

        try
        {
            _fileService.DeleteCaseDocument(document.FilePath, documentFirmId, documentCaseId);
            _logger.LogInformation("File deleted from secure disk storage: {FilePath}", document.FilePath);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to delete file from disk: {FilePath}", document.FilePath);
        }

        var auditLog = _auditService.Create(request.UserID, $"Document Delete: {document.DocumentName} (ID: {document.DocumentID})");

        _context.AuditLogs.Add(auditLog);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Audit log created for document deletion: {DocumentId}", request.DocumentID);

        return true;
    }
}
