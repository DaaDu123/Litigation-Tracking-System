using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using LTSBackend.Features.Cases.DTOs;
using LTSBackend.Models.Cases;
using LTSBackend.Models.Masters;
using LTSBackend.Services.Audit;
using LTSBackend.Services.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LTSBackend.Features.Cases.Commands.CreateCase;

public class CreateCaseHandler(AppDbContext _context, IAuditService _auditService, ILogger<CreateCaseHandler> _logger, IHttpContextAccessor _httpContextAccessor, ICurrentUserService _currentUser) : IRequestHandler<CreateCaseCommand, CreateCaseResultDTO>
{
    // =====================================================
    // HANDLE — registers a brand-new litigation case (SRS UC-01) as ONE
    // consistent business operation.
    //
    // The Case is the central object; everything derived from its Category
    // comes from the Case Workflow Template (firm template first, else the
    // global one): default Department, initial Status, ordered Stages and
    // the required/optional document checklist. Court/Category/Department/
    // Status/Stage/DocumentType are shared master rows - the case only
    // REFERENCES them (nothing is created or duplicated per case). Only the
    // case-owned rows are generated: Case, initial CaseStatusHistory,
    // CaseWorkflowStages (case-specific progress), CaseDocumentRequirements
    // (checklist) and the audit log - all inside ONE transaction, so a
    // failure rolls back everything (no orphan/half-built cases).
    //
    // A category with no template keeps the previous behaviour (status
    // "New", stage "Filing", no checklist) so existing categories keep
    // working until an admin configures their template.
    // =====================================================
    public async Task<CreateCaseResultDTO> Handle(CreateCaseCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating new case: {CaseNumber}", request.CaseNumber);

        int currentUserId = GetCurrentUserId();

        if (_currentUser.FirmID == null)
        {
            _logger.LogWarning("User {UserId} without a firm attempted to create a case", currentUserId);
            throw new ValidationException(["You are not associated with any firm, so you cannot create a case."]);
        }
        int firmId = _currentUser.FirmID.Value;

        // 1. Case Number uniqueness (firm-scoped)
        bool caseExists = await _context.Cases.AsNoTracking().AnyAsync(x => x.CaseNumber == request.CaseNumber && x.FirmID == firmId, cancellationToken);
        if (caseExists)
        {
            _logger.LogWarning("Case Number already exists: {CaseNumber}", request.CaseNumber);
            throw new ValidationException([$"Case Number '{request.CaseNumber}' already exists"]);
        }

        // 2. Court - must already exist in the firm's master data (global or own firm; the query filter enforces that)
        var court = await _context.Courts.AsNoTracking().FirstOrDefaultAsync(x => x.CourtID == request.CourtID && x.IsActive, cancellationToken);
        if (court == null)
        {
            _logger.LogWarning("Court not found or inactive: {CourtID}", request.CourtID);
            throw new ValidationException(["The selected Court is not available for your firm. Courts are configured once in Master Data and then reused."]);
        }

        // 3. Category (active, visible to this firm)
        var category = await _context.CaseCategories.AsNoTracking().FirstOrDefaultAsync(x => x.CategoryID == request.CategoryID && x.IsActive, cancellationToken);
        if (category == null)
        {
            _logger.LogWarning("Category not found or inactive: {CategoryID}", request.CategoryID);
            throw new ValidationException(["The selected Case Category is not available for your firm."]);
        }

        // 4. Resolve the workflow template (firm's own template wins over the global one)
        var templates = await _context.CaseWorkflowTemplates.AsNoTracking()
            .Include(t => t.Stages).ThenInclude(s => s.Stage)
            .Include(t => t.Documents).ThenInclude(d => d.DocumentType)
            .AsSplitQuery()
            .Where(t => t.CategoryID == category.CategoryID && t.IsActive)
            .ToListAsync(cancellationToken);
        var template = templates.FirstOrDefault(t => t.FirmID == firmId) ?? templates.FirstOrDefault(t => t.FirmID == null);

        // 5. Department: explicit pick, else the template's default
        int? departmentId = request.ResponsibleDepartmentID ?? template?.DefaultDepartmentID;
        Department? department = null;
        if (departmentId.HasValue)
        {
            department = await _context.Departments.AsNoTracking().FirstOrDefaultAsync(x => x.DepartmentID == departmentId.Value && x.IsActive, cancellationToken);
            if (department == null)
            {
                _logger.LogWarning("Department not found or inactive: {DepartmentID}", departmentId);
                throw new ValidationException(["The selected Department is not available for your firm."]);
            }
        }

        // 6. Legal Officer (only if provided) must be an active user of this firm
        if (request.CurrentLegalOfficerID.HasValue)
        {
            var legalOfficer = await _context.Users.AsNoTracking().FirstOrDefaultAsync(
                x => x.UserID == request.CurrentLegalOfficerID.Value && x.IsActive && !x.IsDeleted && x.FirmID == firmId, cancellationToken);
            if (legalOfficer == null)
            {
                _logger.LogWarning("Legal Officer not found: {LegalOfficerID}", request.CurrentLegalOfficerID);
                throw new NotFoundException($"Legal Officer ID {request.CurrentLegalOfficerID} not found");
            }
        }

        // 7. Initial Status: template's, else the global "New"
        CaseStatus? status = template != null
            ? await _context.CaseStatuses.AsNoTracking().FirstOrDefaultAsync(x => x.StatusID == template.InitialStatusID && x.IsActive, cancellationToken)
            : await _context.CaseStatuses.AsNoTracking().FirstOrDefaultAsync(x => x.StatusName == "New" && x.IsActive, cancellationToken);
        if (status == null)
        {
            _logger.LogError("Initial status could not be resolved (template: {HasTemplate})", template != null);
            throw new ValidationException([template != null
                ? "The workflow template's initial status is missing or inactive. Please fix the template in Master Data."
                : "Default status 'New' not found"]);
        }

        // 8. Stages: template's active stages in order, else the single legacy "Filing" stage
        var plannedStages = new List<(int StageId, string StageName, int Seq)>();
        if (template != null)
        {
            plannedStages = template.Stages.Where(s => s.Stage.IsActive).OrderBy(s => s.SequenceNo)
                .Select((s, i) => (s.StageID, s.Stage.StageName, i + 1)).ToList();
            if (plannedStages.Count == 0)
                throw new ValidationException(["The workflow template for this category has no active stages. Please fix the template in Master Data."]);
        }
        else
        {
            var filing = await _context.CaseStages.AsNoTracking().FirstOrDefaultAsync(x => x.StageName == "Filing" && x.IsActive, cancellationToken);
            if (filing == null)
                throw new NotFoundException("Default stage 'Filing' not found");
            plannedStages.Add((filing.StageID, filing.StageName, 1));
        }

        var plannedDocs = template == null
            ? new List<CaseWorkflowTemplateDocument>()
            : template.Documents.Where(d => d.DocumentType.IsActive).OrderByDescending(d => d.IsRequired).ThenBy(d => d.DocumentType.TypeName).ToList();

        // 9. One atomic unit of work (CreateExecutionStrategy: required because EnableRetryOnFailure is on)
        var strategy = _context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            // A retried attempt must not see entities left tracked by a failed one (would insert duplicates).
            _context.ChangeTracker.Clear();

            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var now = DateTime.UtcNow;
                string internalRefNo = await GenerateUniqueInternalReferenceNoAsync(cancellationToken);
                var firstStage = plannedStages[0];

                var newCase = new Case
                {
                    FirmID = firmId,
                    InternalReferenceNo = internalRefNo,
                    CaseNumber = request.CaseNumber,
                    CaseTitle = request.CaseTitle,
                    CaseDescription = request.CaseDescription,
                    CourtID = court.CourtID,
                    CategoryID = category.CategoryID,
                    StatusID = status.StatusID,
                    StageID = firstStage.StageId,
                    Priority = request.Priority,
                    SubjectMatter = request.SubjectMatter,
                    FilingDate = request.FilingDate,
                    InstitutionDate = request.InstitutionDate,
                    RegistrationDate = request.RegistrationDate,
                    ExpectedDisposalDate = request.ExpectedDisposalDate,
                    ClaimedAmount = request.ClaimedAmount,
                    PotentialLiability = request.PotentialLiability,
                    FinancialImplication = request.FinancialImplication,
                    ResponsibleDepartmentID = department?.DepartmentID,
                    CurrentLegalOfficerID = request.CurrentLegalOfficerID,
                    CreatedBy = currentUserId,
                    CreatedDate = now,
                    IsClosed = status.IsClosed,
                    IsArchived = false,
                    // Snapshot reference: which template revision this case was generated from (NULL = legacy flow)
                    WorkflowTemplateID = template?.TemplateID,
                    WorkflowTemplateVersion = template?.Version
                };
                _context.Cases.Add(newCase);

                // Initial status history (Case nav so the identity CaseID is fixed up automatically)
                _context.CaseStatusHistories.Add(new CaseStatusHistory
                {
                    Case = newCase,
                    OldStatusID = null,
                    NewStatusID = status.StatusID,
                    ChangedBy = currentUserId,
                    ChangedDate = now,
                    Remarks = "Case created"
                });

                // Case-specific workflow: first stage Active, the rest Pending
                foreach (var st in plannedStages)
                {
                    bool isFirst = st.Seq == 1;
                    _context.CaseWorkflowStages.Add(new CaseWorkflowStage
                    {
                        Case = newCase,
                        StageID = st.StageId,
                        SequenceNo = st.Seq,
                        Status = isFirst ? CaseWorkflowStageState.Active : CaseWorkflowStageState.Pending,
                        StartedDate = isFirst ? now : null
                    });
                }

                // Document checklist (requirements only - no files, no new DocumentType rows)
                foreach (var d in plannedDocs)
                {
                    _context.CaseDocumentRequirements.Add(new CaseDocumentRequirement
                    {
                        Case = newCase,
                        DocumentTypeID = d.DocumentTypeID,
                        IsRequired = d.IsRequired
                    });
                }

                _context.AuditLogs.Add(_auditService.Create(currentUserId, $"Case Create: {newCase.CaseNumber}" + (template != null ? $" (workflow template {template.TemplateID} v{template.Version}, {plannedStages.Count} stage(s), {plannedDocs.Count} checklist item(s))" : " (no workflow template)")));

                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                _logger.LogInformation("Case created: {CaseID} with RefNo: {InternalRefNo}", newCase.CaseID, internalRefNo);

                return new CreateCaseResultDTO
                {
                    CaseID = newCase.CaseID,
                    InternalReferenceNo = internalRefNo,
                    CaseNumber = newCase.CaseNumber,
                    StatusName = status.StatusName,
                    StageName = firstStage.StageName,
                    DepartmentName = department?.DepartmentName,
                    UsedWorkflowTemplate = template != null,
                    Stages = plannedStages.Select(s => new CaseWorkflowStageDTO
                    {
                        StageID = s.StageId,
                        StageName = s.StageName,
                        SequenceNo = s.Seq,
                        Status = s.Seq == 1 ? CaseWorkflowStageState.Active : CaseWorkflowStageState.Pending,
                        StartedDate = s.Seq == 1 ? now : null
                    }).ToList(),
                    Documents = plannedDocs.Select(d => new CaseDocumentRequirementDTO
                    {
                        DocumentTypeID = d.DocumentTypeID,
                        TypeName = d.DocumentType.TypeName,
                        IsRequired = d.IsRequired
                    }).ToList()
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Case create failed and was rolled back: {CaseNumber}", request.CaseNumber);
                throw;
            }
        });
    }

    // SECURITY FIX: see UpdateCaseHandler.GetCurrentUserId for full
    // rationale - previously defaulted to UserID = 1 (SuperAdmin) instead
    // of failing when the identity claim was missing.
    private int GetCurrentUserId()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            _logger.LogWarning("Case create rejected: missing or invalid user identity claim");
            throw new UnauthorizedException("Unable to determine the current user's identity.");
        }

        return userId;
    }

    // Generates a random, collision-checked InternalReferenceNo (retries a
    // few times, then falls back to a GUID-based value if every attempt
    // collides — astronomically unlikely, but keeps the method total).
    private async Task<string> GenerateUniqueInternalReferenceNoAsync(CancellationToken cancellationToken)
    {
        const int maxAttempts = 5;
        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            var candidate = GenerateInternalReferenceNo();
            bool alreadyExists = await _context.Cases.AsNoTracking().AnyAsync(x => x.InternalReferenceNo == candidate, cancellationToken);

            if (!alreadyExists)
            {
                return candidate;
            }

            _logger.LogWarning("InternalReferenceNo collision on attempt {Attempt}: {Candidate}", attempt, candidate);
        }
        return $"CASE-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}"[..24];
    }

    // Builds one candidate reference number: CASE-yyyyMMdd-XXXX.
    private static string GenerateInternalReferenceNo()
    {
        var now = DateTime.UtcNow;
        var randomPart = GenerateRandomString(4);
        return $"CASE-{now:yyyyMMdd}-{randomPart}";
    }

    // Generates a short random alphanumeric suffix for the reference number.
    private static string GenerateRandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Range(0, length).Select(_ => chars[Random.Shared.Next(chars.Length)]).ToArray());
    }
}
