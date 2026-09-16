using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using LTSBackend.Services.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.DocumentTypes.Commands.DeleteDocumentType;

public sealed class DeleteDocumentTypeHandler(AppDbContext _context, ICurrentUserService _currentUser, ILogger<DeleteDocumentTypeHandler> _logger) : IRequestHandler<DeleteDocumentTypeCommand, bool>
{
    // =====================================================
    // HANDLE — removes a document type the firm no longer needs
    // Same ownership check as Update (own firm's custom document type only),
    // plus an in-use check: cannot delete while cases still reference it.
    // =====================================================
    public async Task<bool> Handle(DeleteDocumentTypeCommand request, CancellationToken cancellationToken)
    {
        var type = await _context.DocumentTypes.FirstOrDefaultAsync(x => x.DocumentTypeID == request.DocumentTypeID, cancellationToken);
        if (type == null)
        {
            _logger.LogWarning("Delete failed: Document type not found: {DocumentTypeID}", request.DocumentTypeID);
            throw new NotFoundException("Document type not found.");
        }

        if (type.FirmID != _currentUser.FirmID)
        {
            _logger.LogWarning("Delete denied: user {UserId} attempted to delete a global/other-firm document type {DocumentTypeID}", _currentUser.UserID, request.DocumentTypeID);
            throw new NotFoundException("Document type not found.");
        }

        int documentCount = await _context.Documents.CountAsync(x => x.DocumentTypeID == request.DocumentTypeID, cancellationToken);
        if (documentCount > 0)
        {
            _logger.LogWarning("Delete failed: {Count} document(s) reference type: {DocumentTypeID}", documentCount, request.DocumentTypeID);
            throw new ValidationException(new()
            {
                $"Cannot delete document type. {documentCount} document(s) are currently linked to it. Deactivate it instead."
            });
        }

        _context.DocumentTypes.Remove(type);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Document type deleted successfully: {DocumentTypeID}", request.DocumentTypeID);

        return true;
    }
}
