using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using LTSBackend.Services.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.DocumentTypes.Commands.UpdateDocumentType;

public sealed class UpdateDocumentTypeHandler(AppDbContext _context, ICurrentUserService _currentUser, ILogger<UpdateDocumentTypeHandler> _logger) : IRequestHandler<UpdateDocumentTypeCommand, bool>
{
    // =====================================================
    // HANDLE — edits an existing document type
    // Ownership check: a FirmAdmin may only edit their OWN firm's custom
    // document type — never a global one or another firm's (returns
    // NotFound rather than Forbidden, so its existence isn't disclosed).
    // Also re-checks name uniqueness before saving.
    // =====================================================
    public async Task<bool> Handle(UpdateDocumentTypeCommand request, CancellationToken cancellationToken)
    {
        request = request with { TypeName = request.TypeName.Trim(), Description = request.Description?.Trim() };

        var type = await _context.DocumentTypes.FirstOrDefaultAsync(x => x.DocumentTypeID == request.DocumentTypeID, cancellationToken);
        if (type == null)
        {
            _logger.LogWarning("Update failed: Document type not found: {DocumentTypeID}", request.DocumentTypeID);
            throw new NotFoundException("Document type not found.");
        }

        if (type.FirmID != _currentUser.FirmID)
        {
            _logger.LogWarning("Update denied: user {UserId} attempted to edit a global/other-firm document type {DocumentTypeID}", _currentUser.UserID, request.DocumentTypeID);
            throw new NotFoundException("Document type not found.");
        }

        bool nameExists = await _context.DocumentTypes.AnyAsync(x => x.DocumentTypeID != request.DocumentTypeID && x.TypeName.ToLower() == request.TypeName.ToLower(), cancellationToken);
        if (nameExists)
        {
            _logger.LogWarning("Update failed: Document type name already exists: {TypeName}", request.TypeName);
            throw new ValidationException(new() { $"Document type '{request.TypeName}' already exists." });
        }

        type.TypeName = request.TypeName;
        type.Description = request.Description;
        type.IsActive = request.IsActive;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Document type updated successfully: {DocumentTypeID}", request.DocumentTypeID);

        return true;
    }
}
