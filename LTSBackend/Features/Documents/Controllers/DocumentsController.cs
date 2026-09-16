using LTSBackend.Comman.Exceptions;
using LTSBackend.Comman.Responses;
using LTSBackend.Features.Authorization;
using LTSBackend.Features.Documents.Commands.ApproveDocument;
using LTSBackend.Features.Documents.Commands.DeleteDocument;
using LTSBackend.Features.Documents.Commands.DownloadDocument;
using LTSBackend.Features.Documents.Commands.UploadDocument;
using LTSBackend.Features.Documents.DTOs;
using LTSBackend.Features.Documents.Queries.GetCaseDocuments;
using LTSBackend.Features.Documents.Queries.GetDocument;
using LTSBackend.Models.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LTSBackend.Features.Documents.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class DocumentsController(IMediator _mediator, ILogger<DocumentsController> _logger) : ControllerBase
{
    // =====================================================
    // UPLOAD DOCUMENT — CanViewDocuments roles, InternParalegal, FirmAdmin and above
    // Uploads a file (max 15MB) against a case. Restricted-mode Moharrir
    // "blind upload" (write-only, View/Download disabled after success)
    // and Intern draft-only rules are enforced downstream in the handler,
    // not here — this endpoint just validates the file is present/sized
    // correctly and forwards the acting user's own ID (from their JWT
    // claim) so those rules can be applied.
    // =====================================================
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    [Authorize(Roles = RoleNames.CanViewDocuments + "," + RoleNames.InternParalegal + "," + RoleNames.FirmAdminAndAbove)]
    public async Task<IActionResult> UploadDocument(
        [FromForm] UploadDocumentRequest request)
    {
        _logger.LogInformation("Upload document request - Case: {CaseId}, Type: {TypeId}, File: {FileName}",
            request.CaseID,
            request.DocumentTypeID,
            request.File?.FileName);

        if (request.File == null || request.File.Length == 0)
        {
            return BadRequest(ApiResponse<bool>.FailureResponse("File is required"));
        }

        if (request.File.Length > 15 * 1024 * 1024)
        {
            return BadRequest(ApiResponse<bool>.FailureResponse("File size is too large. Please reduce the document size to 15MB or less."));
        }

        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(ApiResponse<bool>.FailureResponse("Invalid user identity"));
        }

        var command = new UploadDocumentCommand(request.CaseID, request.DocumentTypeID, request.DocumentName, request.File, request.Remarks)
        {
            UserID = userId
        };

        try
        {
            var result = await _mediator.Send(command);

            _logger.LogInformation("Document uploaded successfully - ID: {DocumentId}, Restricted: {Restricted}",
                result.DocumentID, result.IsRestrictedMohallirUpload);

            var response = new UploadDocumentResponseDTO
            {
                DocumentID = result.DocumentID,
                CaseID = request.CaseID,
                DocumentName = request.DocumentName,
                IsRestrictedMohallirUpload = result.IsRestrictedMohallirUpload,
                Message = result.IsRestrictedMohallirUpload
                    ? "Document uploaded successfully (restricted - you cannot view/download this file)"
                    : "Document uploaded successfully"
            };

            return Ok(ApiResponse<UploadDocumentResponseDTO>.SuccessResponse(
                response,
                response.Message));
        }
        catch (UnauthorizedException ex)
        {
            _logger.LogWarning(ex, "Upload unauthorized for user {UserId}", userId);
            return Forbid();
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Upload failed - resource not found");
            return NotFound(ApiResponse<bool>.FailureResponse(ex.Message));
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Upload validation failed for user {UserId}", userId);
            return BadRequest(ApiResponse<bool>.FailureResponse(string.Join(" ", ex.Errors)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Document upload failed");
            return BadRequest(ApiResponse<bool>.FailureResponse($"Upload failed: {ex.Message}"));
        }
    }

    // =====================================================
    // GET CASE DOCUMENTS — Any authenticated user (visibility enforced in handler)
    // Lists the latest-version documents on a case that the CURRENT user
    // is allowed to view — a Restricted-mode Moharrir, for instance, won't
    // see files they aren't permitted to view/download. Filtering happens
    // per-document inside GetCaseDocumentsQueryHandler.
    // =====================================================
    [HttpGet("case/{caseId}")]
    public async Task<IActionResult> GetCaseDocuments(long caseId)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(ApiResponse<bool>.FailureResponse("Invalid user identity"));
        }

        var result = await _mediator.Send(new GetCaseDocumentsQuery(caseId) { UserID = userId });
        return Ok(ApiResponse<List<DocumentDetailDTO>>.SuccessResponse(result, "Case documents fetched"));
    }

    // =====================================================
    // DOWNLOAD DOCUMENT — Any authenticated user (permission enforced in handler)
    // Streams a document's raw file bytes back to the caller, but only if
    // DownloadDocumentCommand's permission check passes for this specific
    // user/document (e.g. a Restricted-mode Moharrir is blocked here even
    // if they can see the document in the list).
    // =====================================================
    [HttpGet("download/{documentId}")]
    public async Task<IActionResult> DownloadDocument(long documentId)
    {
        _logger.LogInformation("Download document request - ID: {DocumentId}", documentId);

        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(ApiResponse<bool>.FailureResponse("Invalid user identity"));
        }

        try
        {
            var command = new DownloadDocumentCommand(documentId) { UserID = userId };
            var downloadData = await _mediator.Send(command);

            return File(downloadData.FileBytes, downloadData.ContentType, downloadData.FileName);
        }
        catch (UnauthorizedException ex)
        {
            _logger.LogWarning(ex, "Download unauthorized for user {UserId} document {DocumentId}", userId, documentId);
            return Forbid();
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Download failed - document not found {DocumentId}", documentId);
            return NotFound(ApiResponse<bool>.FailureResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Document download failed");
            return BadRequest(ApiResponse<bool>.FailureResponse($"Download failed: {ex.Message}"));
        }
    }

    // =====================================================
    // GET DOCUMENT METADATA — Any authenticated user (permission enforced in handler)
    // Returns a single document's metadata (name, type, version, uploader,
    // upload date, etc.) — not the file bytes themselves — provided the
    // caller has view permission on it.
    // =====================================================
    [HttpGet("{documentId}")]
    public async Task<IActionResult> GetDocument(long documentId)
    {
        _logger.LogInformation("Get document request - ID: {DocumentId}", documentId);

        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(ApiResponse<bool>.FailureResponse("Invalid user identity"));
        }

        try
        {
            var query = new GetDocumentQuery(documentId) { UserID = userId };
            var document = await _mediator.Send(query);

            if (document == null)
            {
                return NotFound(ApiResponse<bool>.FailureResponse("Document not found"));
            }

            return Ok(ApiResponse<DocumentDetailDTO>.SuccessResponse(document, "Document retrieved successfully"));
        }
        catch (UnauthorizedException ex)
        {
            _logger.LogWarning(ex, "View unauthorized for user {UserId} document {DocumentId}", userId, documentId);
            return Forbid();
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Get document failed - not found {DocumentId}", documentId);
            return NotFound(ApiResponse<bool>.FailureResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve document");
            return BadRequest(ApiResponse<bool>.FailureResponse($"Failed to retrieve document: {ex.Message}"));
        }
    }

    // =====================================================
    // APPROVE DOCUMENT — Partner and above
    // Publishes a pending draft document (e.g. one uploaded by an Intern
    // in draft-only mode) so it becomes a normal, visible document on the
    // case.
    // =====================================================
    [HttpPost("{documentId}/approve")]
    [Authorize(Roles = RoleNames.PartnerAndAbove)]
    public async Task<IActionResult> ApproveDocument(long documentId)
    {
        _logger.LogInformation("Approve document request - ID: {DocumentId}", documentId);

        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(ApiResponse<bool>.FailureResponse("Invalid user identity"));
        }

        try
        {
            var command = new ApproveDocumentCommand(documentId) { UserID = userId };
            var result = await _mediator.Send(command);

            return Ok(ApiResponse<bool>.SuccessResponse(result, "Document approved successfully"));
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Approve failed - document not found {DocumentId}", documentId);
            return NotFound(ApiResponse<bool>.FailureResponse(ex.Message));
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Approve failed - validation error {DocumentId}", documentId);
            return BadRequest(ApiResponse<bool>.FailureResponse(string.Join(" ", ex.Errors)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Document approval failed");
            return BadRequest(ApiResponse<bool>.FailureResponse($"Approval failed: {ex.Message}"));
        }
    }

    // =====================================================
    // DELETE DOCUMENT — Partner and above
    // Hard-deletes a document record, its stored file, and its
    // DocumentPermissions rows. Not reversible.
    // =====================================================
    [HttpDelete("{documentId}")]
    [Authorize(Roles = RoleNames.PartnerAndAbove)]
    public async Task<IActionResult> DeleteDocument(long documentId)
    {
        _logger.LogInformation("Delete document request - ID: {DocumentId}", documentId);

        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(ApiResponse<bool>.FailureResponse("Invalid user identity"));
        }

        try
        {
            var command = new DeleteDocumentCommand(documentId) { UserID = userId };
            var result = await _mediator.Send(command);

            return Ok(ApiResponse<bool>.SuccessResponse(result, "Document deleted successfully"));
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Delete failed - document not found {DocumentId}", documentId);
            return NotFound(ApiResponse<bool>.FailureResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Document deletion failed");
            return BadRequest(ApiResponse<bool>.FailureResponse($"Deletion failed: {ex.Message}"));
        }
    }
}
