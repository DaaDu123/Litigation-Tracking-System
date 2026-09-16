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
    // =====================================================
    // HANDLE — checks download permission and streams the file back
    // Calls CanUserAccessDocumentAsync (which is where a Restricted-mode
    // Moharrir gets blocked even if they can see the document in a list),
    // then reads the raw bytes from secure disk storage and returns them
    // with a resolved content type.
    // =====================================================
    public async Task<DocumentDownloadDTO> Handle(DownloadDocumentCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Document download request - ID: {DocumentId}, User: {UserId}",request.DocumentID,request.UserID);

        bool canDownload = await _permissionService.CanUserAccessDocumentAsync(request.UserID, request.DocumentID, "Download", cancellationToken);

        if (!canDownload)
        {
            _logger.LogWarning("User {UserId} denied download access to document {DocumentId}", request.UserID, request.DocumentID);
            throw new UnauthorizedException("You don't have permission to download this document. " + "If you are a restricted Moharrir, contact your administrator to grant access.");
        }

        // Include Case so we have its FirmID for the storage-layer ownership check below.
        // (Document itself is also tenant-filtered by EF Core's HasQueryFilter on
        // Case.FirmID == RequestFirmId, so this Include also re-confirms that filter fired.)
        var document = await _context.Documents.AsNoTracking().Include(x => x.Case).FirstOrDefaultAsync(x => x.DocumentID == request.DocumentID, cancellationToken);

        if (document == null)
        {
            _logger.LogWarning("Document not found: {DocumentId}", request.DocumentID);
            throw new NotFoundException($"Document {request.DocumentID} not found");
        }

        // Defense-in-depth: re-confirm the document's own case belongs to the
        // caller's firm even though the EF query filter already enforces this,
        // before we ever touch the file on disk. SuperAdmin is platform-level
        // (FirmID is null on their own account) and is intentionally exempt,
        // same as the EF Core tenant query filter's BypassTenantFilter rule.
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

    // Maps a file extension to its HTTP content type for the download response.
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
