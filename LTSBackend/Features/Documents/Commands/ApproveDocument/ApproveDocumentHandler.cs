using LTSBackend.Comman.Enum;
using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using LTSBackend.Services.Audit;
using LTSBackend.Services.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.Documents.Commands.ApproveDocument;

public class ApproveDocumentHandler(AppDbContext _context, IAuditService _auditService, ICurrentUserService _currentUser, ILogger<ApproveDocumentHandler> _logger) : IRequestHandler<ApproveDocumentCommand, bool>
{
    // =====================================================
    // HANDLE — approves a pending draft document (Partner and above)
    // Firm-scoped lookup, refuses if the document isn't actually a
    // pending draft, then flips IsDraft off and stamps who approved it
    // and when, making it visible to the rest of the case team.
    // =====================================================
    public async Task<bool> Handle(ApproveDocumentCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Approve document attempt - ID: {DocumentId}, User: {UserId}", request.DocumentID, request.UserID);

        var document = await _context.Documents.Include(d => d.Case).FirstOrDefaultAsync(d => d.DocumentID == request.DocumentID, cancellationToken);

        if (document == null)
        {
            _logger.LogWarning("Approve failed: Document not found {DocumentId}", request.DocumentID);
            throw new NotFoundException($"Document {request.DocumentID} not found");
        }

        if (document.Case == null || document.Case.FirmID != _currentUser.FirmID)
        {
            _logger.LogWarning("Approve denied: cross-firm access blocked for document {DocumentId}", request.DocumentID);
            throw new NotFoundException($"Document {request.DocumentID} not found");
        }

        if (!document.IsDraft)
        {
            _logger.LogWarning("Approve failed: Document {DocumentId} is not a pending draft", request.DocumentID);
            throw new ValidationException(["This document is not pending approval"]);
        }

        document.IsDraft = false;
        document.ApprovedBy = request.UserID;
        document.ApprovedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        var auditLog = _auditService.Create(request.UserID, $"Document Approved: {document.DocumentName} (ID: {document.DocumentID})");
        _context.AuditLogs.Add(auditLog);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Document {DocumentId} approved by user {UserId}", request.DocumentID, request.UserID);
        return true;
    }
}
