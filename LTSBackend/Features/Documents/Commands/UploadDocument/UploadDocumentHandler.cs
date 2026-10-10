using LTSBackend.Comman.Enum;
using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using LTSBackend.Models.Cases;
using LTSBackend.Services.Audit;
using LTSBackend.Services.CurrentUser;
using LTSBackend.Services.DocumentPermissions;
using LTSBackend.Services.ProfileService;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.Documents.Commands.UploadDocument;

public class UploadDocumentHandler(AppDbContext _context, IFileService _fileService, IDocumentPermissionService _permissionService, IAuditService _auditService,
    ICurrentUserService _currentUser, ILogger<UploadDocumentHandler> _logger) : IRequestHandler<UploadDocumentCommand, UploadDocumentResult>
{
    // =====================================================
    // HANDLE — uploads a file and applies role-based document permissions
    // Validates the user, the CanUserUploadToCaseAsync permission, the
    // case (firm-scoped), and the document type, then saves the file to
    // secure disk storage and records the Document row. An Intern's
    // upload is marked IsDraft = true (pending approval). Then grants
    // per-role DocumentPermissions: a Restricted-mode Moharrir gets NO
    // view/download grant at all ("blind upload" — write-only), while
    // everyone else gets a role-appropriate view/download grant.
    //
    // FAILURE SAFETY: the file is written first, then the Document row,
    // permission grant and audit line run in ONE transaction. If anything
    // after the file write fails, the transaction rolls back (no half-saved
    // document without permissions) AND the just-written file is deleted, so
    // no orphan file is left on disk and the client can simply retry.
    // =====================================================
    public async Task<UploadDocumentResult> Handle(UploadDocumentCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Document upload started for case {CaseId} by user {UserId}", request.CaseID, request.UserID);

        var user = await _context.Users.AsNoTracking().Include(x => x.Role).FirstOrDefaultAsync(x => x.UserID == request.UserID, cancellationToken);

        if (user == null)
        {
            _logger.LogWarning("Upload failed: User not found {UserId}", request.UserID);
            throw new NotFoundException($"User {request.UserID} not found");
        }

        bool canUpload = await _permissionService.CanUserUploadToCaseAsync(request.UserID, request.CaseID, cancellationToken);
        if (!canUpload)
        {
            _logger.LogWarning("Upload denied: User {UserId} cannot upload to case {CaseId}", request.UserID, request.CaseID);
            throw new UnauthorizedException("You don't have permission to upload documents to this case");
        }

        var caseRecord = await _context.Cases.AsNoTracking().FirstOrDefaultAsync(x => x.CaseID == request.CaseID, cancellationToken);

        if (caseRecord == null || (caseRecord.FirmID != _currentUser.FirmID))
        {
            _logger.LogWarning("Upload failed: Case not found or cross-firm access blocked {CaseId}", request.CaseID);
            throw new NotFoundException($"Case {request.CaseID} not found");
        }

        if (caseRecord.IsArchived)
        {
            _logger.LogWarning("Upload refused: case {CaseId} is archived", request.CaseID);
            throw new ValidationException(["This case is archived. Restore it before uploading documents."]);
        }

        var documentType = await _context.DocumentTypes.AsNoTracking().FirstOrDefaultAsync(x => x.DocumentTypeID == request.DocumentTypeID, cancellationToken);

        if (documentType == null)
        {
            _logger.LogWarning("Upload failed: Document type not found {TypeId}", request.DocumentTypeID);
            throw new NotFoundException($"Document type {request.DocumentTypeID} not found");
        }

        string filePath;
        try
        {
            // Tenant/case-isolated storage: Firm/{FirmID}/Case/{CaseID}/Documents/.
            // caseRecord.FirmID was already confirmed to match the caller's own
            // firm above, so this can never write into another firm's folder.
            filePath = await _fileService.SaveCaseDocumentAsync(request.File, caseRecord.FirmID, request.CaseID);
            _logger.LogInformation("File saved to secure disk storage: {FilePath}", filePath);
        }
        catch (ValidationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save file for case {CaseId}", request.CaseID);
            throw new InvalidOperationException("Failed to save document file");
        }

        bool isInternUpload = user.GetRole() == UserRole.InternParalegal;
        bool isMohallirRestricted = false;

        try
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            long documentId = await strategy.ExecuteAsync(async () =>
            {
                // A retried attempt must not see entities left tracked by a failed one.
                _context.ChangeTracker.Clear();

                await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                try
                {
                    var document = new Document
                    {
                        CaseID = request.CaseID,
                        DocumentTypeID = request.DocumentTypeID,
                        DocumentName = request.DocumentName,
                        FileName = request.File.FileName,
                        FilePath = filePath,
                        FileSize = request.File.Length,
                        VersionNo = 1,
                        UploadedBy = request.UserID,
                        UploadedDate = DateTime.UtcNow,
                        IsLatest = true,
                        Remarks = request.Remarks,
                        IsDraft = isInternUpload
                    };

                    _context.Documents.Add(document);
                    await _context.SaveChangesAsync(cancellationToken);

                    _logger.LogInformation("Document created with ID {DocumentId} for case {CaseId}", document.DocumentID, request.CaseID);

                    isMohallirRestricted = await _permissionService.IsMohallirRestrictedAsync(request.UserID, cancellationToken);
                    if (isMohallirRestricted)
                    {
                        _logger.LogInformation("Moharrir {UserId} blind upload: Document {DocumentId} - no view/download permissions granted",
                            request.UserID, document.DocumentID);
                    }
                    else
                    {
                        var role = user.Role;
                        if (role != null)
                        {
                            bool canView = true;
                            bool canDownload = user.GetRole() switch
                            {
                                UserRole.Partner => true,
                                UserRole.AssociateLawyer => true,
                                UserRole.Moharrir => true,
                                UserRole.InternParalegal => false,
                                _ => false
                            };

                            await _permissionService.GrantDocumentPermissionAsync(document.DocumentID, role.RoleID, canView, canDownload, true, cancellationToken);

                            _logger.LogInformation("Document permissions granted for role {RoleId}: View={CanView}, Download={CanDownload}",
                                role.RoleID, canView, canDownload);
                        }
                    }

                    var auditLog = _auditService.Create(request.UserID, $"Document Upload: {document.DocumentName} to Case {request.CaseID}" + (isInternUpload ? " (Draft - pending approval)" : ""));

                    _context.AuditLogs.Add(auditLog);
                    await _context.SaveChangesAsync(cancellationToken);

                    await transaction.CommitAsync(cancellationToken);
                    return document.DocumentID;
                }
                catch
                {
                    await transaction.RollbackAsync(cancellationToken);
                    throw;
                }
            });

            _logger.LogInformation("Document upload completed - ID: {DocumentId}, User: {UserId}, Case: {CaseId}", documentId, request.UserID, request.CaseID);
            return new UploadDocumentResult(documentId, isMohallirRestricted);
        }
        catch (Exception ex)
        {
            // DB work failed and was rolled back -> remove the file we just wrote so nothing is orphaned.
            _logger.LogError(ex, "Document upload failed after the file was saved; removing orphan file {FilePath}", filePath);
            try
            {
                _fileService.DeleteCaseDocument(filePath, caseRecord.FirmID, request.CaseID);
            }
            catch (Exception cleanupEx)
            {
                _logger.LogError(cleanupEx, "Could not remove orphan file {FilePath} - manual cleanup needed", filePath);
            }
            throw;
        }
    }
}
