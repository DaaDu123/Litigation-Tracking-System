using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using LTSBackend.Models.Cases;
using LTSBackend.Services.Audit;
using LTSBackend.Services.CurrentUser;
using LTSBackend.Services.ProfileService;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LTSBackend.Features.Cases.Commands.DeleteCase;

public class DeleteCaseHandler(AppDbContext _context, IAuditService _auditService, ILogger<DeleteCaseHandler> _logger, IHttpContextAccessor _httpContextAccessor, ICurrentUserService _currentUser, IFileService _fileService) : IRequestHandler<DeleteCaseCommand, bool>
{



















    public async Task<bool> Handle(DeleteCaseCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting case: {CaseID}", request.CaseID);

        int currentUserId = GetCurrentUserId();


        var caseQuery = _context.Cases.Where(x => x.CaseID == request.CaseID);
            caseQuery = caseQuery.Where(x => x.FirmID == _currentUser.FirmID);
        var caseToDelete = await caseQuery.FirstOrDefaultAsync(cancellationToken);

        if (caseToDelete == null)
        {
            _logger.LogWarning("Case not found: {CaseID}", request.CaseID);
            throw new NotFoundException($"Case ID {request.CaseID} not found");
        }


        if (!caseToDelete.IsArchived)
        {
            _logger.LogWarning("Permanent delete refused, case not archived: {CaseID}", request.CaseID);
            throw new ValidationException(["Only an archived case can be permanently deleted. Archive it first."]);
        }

        if (!string.Equals(request.ConfirmCaseNumber?.Trim(), caseToDelete.CaseNumber?.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Permanent delete refused, confirmation mismatch: {CaseID}", request.CaseID);
            throw new ValidationException(["The case number you typed does not match. Permanent deletion cancelled."]);
        }


        var filesToDelete = await _context.Documents
            .Where(x => x.CaseID == request.CaseID)
            .Select(x => x.FilePath)
            .ToListAsync(cancellationToken);
        int firmIdForFiles = caseToDelete.FirmID;
        string caseNumber = caseToDelete.CaseNumber;




        var strategy = _context.Database.CreateExecutionStrategy();

        bool deleted = await strategy.ExecuteAsync(async () =>
        {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {








            var hearingIds = await _context.Hearings.Where(x => x.CaseID == request.CaseID).Select(x => x.HearingID).ToListAsync(cancellationToken);

            if (hearingIds.Count > 0)
            {
                var attendance = await _context.Set<HearingAttendance>().Where(x => hearingIds.Contains(x.HearingID)).ToListAsync(cancellationToken);

                if (attendance.Count > 0)
                {
                    _context.Set<HearingAttendance>().RemoveRange(attendance);
                }
            }

            var hearings = await _context.Hearings.Where(x => x.CaseID == request.CaseID).ToListAsync(cancellationToken);
            if (hearings.Count > 0)
            {
                _context.Hearings.RemoveRange(hearings);
            }


            var documentIds = await _context.Documents.Where(x => x.CaseID == request.CaseID).Select(x => x.DocumentID).ToListAsync(cancellationToken);

            if (documentIds.Count > 0)
            {
                var docPermissions = await _context.DocumentPermissions.Where(x => documentIds.Contains(x.DocumentID)).ToListAsync(cancellationToken);

                if (docPermissions.Count > 0)
                {
                    _context.DocumentPermissions.RemoveRange(docPermissions);
                }
            }

            var documents = await _context.Documents.Where(x => x.CaseID == request.CaseID).ToListAsync(cancellationToken);
            if (documents.Count > 0)
            {
                _context.Documents.RemoveRange(documents);
            }


            var parties = await _context.CaseParties.Where(x => x.CaseID == request.CaseID).ToListAsync(cancellationToken);
            if (parties.Count > 0) _context.CaseParties.RemoveRange(parties);

            var assignments = await _context.CaseAssignments.Where(x => x.CaseID == request.CaseID).ToListAsync(cancellationToken);
            if (assignments.Count > 0) _context.CaseAssignments.RemoveRange(assignments);

            var deadlines = await _context.Deadlines.Where(x => x.CaseID == request.CaseID).ToListAsync(cancellationToken);
            if (deadlines.Count > 0) _context.Deadlines.RemoveRange(deadlines);

            var milestones = await _context.CaseMilestones.Where(x => x.CaseID == request.CaseID).ToListAsync(cancellationToken);
            if (milestones.Count > 0) _context.CaseMilestones.RemoveRange(milestones);


            var workflowStages = await _context.CaseWorkflowStages.Where(x => x.CaseID == request.CaseID).ToListAsync(cancellationToken);
            if (workflowStages.Count > 0) _context.CaseWorkflowStages.RemoveRange(workflowStages);

            var docRequirements = await _context.CaseDocumentRequirements.Where(x => x.CaseID == request.CaseID).ToListAsync(cancellationToken);
            if (docRequirements.Count > 0) _context.CaseDocumentRequirements.RemoveRange(docRequirements);

            var statusHistories = await _context.CaseStatusHistories.Where(x => x.CaseID == request.CaseID).ToListAsync(cancellationToken);
            if (statusHistories.Count > 0) _context.CaseStatusHistories.RemoveRange(statusHistories);

            var notes = await _context.CaseNotes.Where(x => x.CaseID == request.CaseID).ToListAsync(cancellationToken);
            if (notes.Count > 0) _context.CaseNotes.RemoveRange(notes);

            var notifications = await _context.Notifications.Where(x => x.CaseID == request.CaseID).ToListAsync(cancellationToken);
            if (notifications.Count > 0) _context.Notifications.RemoveRange(notifications);


            await _context.SaveChangesAsync(cancellationToken);


            _context.Cases.Remove(caseToDelete);


            var auditLog = _auditService.Create(currentUserId, $"Case PERMANENT Delete: {caseToDelete.CaseNumber} (ID {request.CaseID}, {filesToDelete.Count} document file(s)) — reason: {request.Reason.Trim()}");
            _context.AuditLogs.Add(auditLog);


            await _context.SaveChangesAsync(cancellationToken);


            await transaction.CommitAsync(cancellationToken);

            _logger.LogInformation("Case successfully deleted: {CaseID}", request.CaseID);

            return true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Case delete fail ho gya: {CaseID}", request.CaseID);
            throw;
        }
        });



        foreach (var path in filesToDelete)
        {
            try
            {
                _fileService.DeleteCaseDocument(path, firmIdForFiles, request.CaseID);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Case {CaseNumber} deleted but file could not be removed: {Path}", caseNumber, path);
            }
        }

        return deleted;
    }



    private int GetCurrentUserId()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            _logger.LogWarning("Case delete rejected: missing or invalid user identity claim");
            throw new UnauthorizedException("Unable to determine the current user's identity.");
        }

        return userId;
    }
}