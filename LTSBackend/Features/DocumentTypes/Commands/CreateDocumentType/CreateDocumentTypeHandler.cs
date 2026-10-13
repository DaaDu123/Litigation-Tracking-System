using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using LTSBackend.Models.Masters;
using LTSBackend.Services.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.DocumentTypes.Commands.CreateDocumentType;

public sealed class CreateDocumentTypeHandler(AppDbContext _context, ICurrentUserService _currentUser, ILogger<CreateDocumentTypeHandler> _logger) : IRequestHandler<CreateDocumentTypeCommand, int>
{







    public async Task<int> Handle(CreateDocumentTypeCommand request, CancellationToken cancellationToken)
    {
        request = request with { TypeName = request.TypeName.Trim(), Description = request.Description?.Trim() };

        bool exists = await _context.DocumentTypes.AnyAsync(x => x.TypeName.ToLower() == request.TypeName.ToLower(), cancellationToken);
        if (exists)
        {
            _logger.LogWarning("Create failed: Document type already exists: {TypeName}", request.TypeName);
            throw new ValidationException(new() { $"Document type '{request.TypeName}' already exists." });
        }

        var type = new DocumentType
        {
            FirmID = _currentUser.FirmID,
            TypeName = request.TypeName,
            Description = request.Description,
            IsActive = request.IsActive
        };

        _context.DocumentTypes.Add(type);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Document type created successfully: {DocumentTypeID}", type.DocumentTypeID);

        return type.DocumentTypeID;
    }
}
