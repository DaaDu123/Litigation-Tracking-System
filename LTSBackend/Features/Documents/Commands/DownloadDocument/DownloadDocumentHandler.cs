using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using LTSBackend.Services.CurrentUser;
using LTSBackend.Services.DocumentPermissions;
using LTSBackend.Services.ProfileService;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.Documents.Commands.DownloadDocument;

public class DownloadDocumentHandler(AppDbContext _context, IDocumentPermissionService _permissionService, IFileService _fileService,
    ICurrentUserService _currentUser, ILogger<DownloadDocumentHandler> _logger) : IRequestHandler<DownloadDocumentCommand, DocumentDownloadDTO>
{







    public async Task<DocumentDownloadDTO> Handle(DownloadDocumentCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Document download request - ID: {DocumentId}, User: {UserId}",request.DocumentID,request.UserID);

        bool canDownload = await _permissionService.CanUserAccessDocumentAsync(request.UserID, request.DocumentID, "Download", cancellationToken);

        if (!canDownload)
        {
            _logger.LogWarning("User {UserId} denied download access to document {DocumentId}", request.UserID, request.DocumentID);
            throw new UnauthorizedException("You don't have permission to download this document. " + "If you are a restricted Moharrir, contact your administrator to grant access.");
        }




        var document = await _context.Documents.AsNoTracking().Include(x => x.Case).FirstOrDefaultAsync(x => x.DocumentID == request.DocumentID, cancellationToken);

        if (document == null)
        {
            _logger.LogWarning("Document not found: {DocumentId}", request.DocumentID);
            throw new NotFoundException($"Document {request.DocumentID} not found");
        }






        if (document.Case == null || (!_currentUser.IsSuperAdmin && document.Case.FirmID != _currentUser.FirmID))
        {
            _logger.LogWarning("Download denied: document {DocumentId} does not belong to the caller's firm", request.DocumentID);
            throw new NotFoundException($"Document {request.DocumentID} not found");
        }

        byte[] fileBytes;
        try
        {
            fileBytes = await _fileService.ReadCaseDocumentAsync(document.FilePath, document.Case.FirmID, document.CaseID);
            _logger.LogInformation("Document file read successfully: {DocumentId}, Size: {Size} bytes",request.DocumentID,fileBytes.Length);
        }
        catch (FileNotFoundException ex)
        {
            _logger.LogError(ex, "File not found on disk for document {DocumentId}: {FilePath}", request.DocumentID, document.FilePath);
            throw new InvalidOperationException("Document file not found on server");
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogError(ex, "Cross-tenant path mismatch for document {DocumentId}: {FilePath}", request.DocumentID, document.FilePath);
            throw new UnauthorizedException("You don't have permission to download this document.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read document file: {FilePath}", document.FilePath);
            throw new InvalidOperationException("Failed to read document file");
        }

        var downloadDto = new DocumentDownloadDTO
        {
            DocumentID = document.DocumentID,
            FileName = document.FileName,
            FileBytes = fileBytes,
            ContentType = GetContentType(document.FileName),
            FileSize = fileBytes.Length
        };

        _logger.LogInformation("Document download prepared: {DocumentId}, FileName: {FileName}", request.DocumentID, document.FileName);
        return downloadDto;
    }


    private static string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();

        return extension switch
        {
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".txt" => "text/plain",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".zip" => "application/zip",
            _ => "application/octet-stream"
        };
    }
}
