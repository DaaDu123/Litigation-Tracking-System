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







    public async Task<bool> Handle(DeleteDocumentCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Document delete attempt - ID: {DocumentId}, User: {UserId}", request.DocumentID, request.UserID);




        var document = await _context.Documents.Include(x => x.Case).FirstOrDefaultAsync(x => x.DocumentID == request.DocumentID, cancellationToken);

        if (document == null)
        {
            _logger.LogWarning("Delete failed: Document not found {DocumentId}", request.DocumentID);
            throw new NotFoundException($"Document {request.DocumentID} not found");
        }







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
