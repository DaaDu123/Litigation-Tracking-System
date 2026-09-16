using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using LTSBackend.Models.Cases;
using LTSBackend.Services.Audit;
using LTSBackend.Services.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LTSBackend.Features.Cases.Commands.CreateCase;

public class CreateCaseHandler(AppDbContext _context, IAuditService _auditService, ILogger<CreateCaseHandler> _logger, IHttpContextAccessor _httpContextAccessor, ICurrentUserService _currentUser) : IRequestHandler<CreateCaseCommand, long>
{
    // =====================================================
    // HANDLE — registers a brand-new litigation case (SRS UC-01)
    // Resolves/creates the Court, Category, and Department from either a
    // picked ID or a typed name (type-or-select form fields), validates
    // case-number uniqueness within the firm, defaults Status="New" and
    // Stage="Filing", generates a unique InternalReferenceNo, then saves
    // the case plus its initial CaseStatusHistory row and an audit log
    // entry. Date-sanity (e.g. Expected Disposal Date not in the past) is
    // already enforced by CreateCaseValidator before this ever runs.
    // =====================================================
    public async Task<long> Handle(CreateCaseCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating new case: {CaseNumber}", request.CaseNumber);

        // Get logged-in user ID + firm (multi-tenancy)
        int currentUserId = GetCurrentUserId();

        if (_currentUser.FirmID == null)
        {
            _logger.LogWarning("User {UserId} without a firm attempted to create a case", currentUserId);
            throw new ValidationException(["You are not associated with any firm, so you cannot create a case."]);
        }
        int firmId = _currentUser.FirmID.Value;

        // 1. Case Number uniqueness check (firm-scoped)
        bool caseExists = await _context.Cases.AsNoTracking().AnyAsync(x => x.CaseNumber == request.CaseNumber && x.FirmID == firmId, cancellationToken);

        if (caseExists)
        {
            _logger.LogWarning("Case Number already exists: {CaseNumber}", request.CaseNumber);
            throw new ValidationException(new List<string>
            {
                $"Case Number '{request.CaseNumber}' already exists"
            });
        }

        // Court: use the picked ID, or resolve/create by the typed name
        // (type-or-select combo box on the Create Case form).
        int resolvedCourtId = await GetOrCreateCourtIdAsync(request.CourtID, request.CourtName, firmId, cancellationToken);

        // 3. Category: use the picked ID, or resolve/create by the typed name
        int resolvedCategoryId = await GetOrCreateCategoryIdAsync(request.CategoryID, request.CategoryName, firmId, cancellationToken);

        // 4. Department: same type-or-select pattern (optional field)
        int? resolvedDepartmentId = await GetOrCreateDepartmentIdAsync(request.ResponsibleDepartmentID, request.DepartmentName, firmId, cancellationToken);

        // 5. Legal Officer check — ONLY if a value was provided
        if (request.CurrentLegalOfficerID.HasValue)
        {
            var legalOfficer = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.UserID == request.CurrentLegalOfficerID.Value &&
                         x.IsActive &&
                         !x.IsDeleted &&
                         x.FirmID == firmId,
                    cancellationToken);

            if (legalOfficer == null)
            {
                _logger.LogWarning("Legal Officer not found: {LegalOfficerID}", request.CurrentLegalOfficerID);
                throw new NotFoundException($"Legal Officer ID {request.CurrentLegalOfficerID} not found");
            }
        }

        // 6. Default Status = "New"
        var defaultStatus = await _context.CaseStatuses.AsNoTracking().FirstOrDefaultAsync(x => x.StatusName == "New", cancellationToken);

        if (defaultStatus == null)
        {
            _logger.LogError("Default status 'New' not found in the database");
            throw new NotFoundException("Default status 'New' not found");
        }

        // 7. Default Stage = "Filing"
        var defaultStage = await _context.CaseStages.AsNoTracking().FirstOrDefaultAsync(x => x.StageName == "Filing", cancellationToken);

        if (defaultStage == null)
        {
            _logger.LogError("Default stage 'Filing' not found in the database");
            throw new NotFoundException("Default stage 'Filing' not found");
        }

        // 8. Unique Internal Reference Number generate karo
        string internalRefNo = await GenerateUniqueInternalReferenceNoAsync(cancellationToken);

        // 9. Build the Case object
        var newCase = new Case
        {
            FirmID = firmId,
            InternalReferenceNo = internalRefNo,
            CaseNumber = request.CaseNumber,
            CaseTitle = request.CaseTitle,
            CaseDescription = request.CaseDescription,
            CourtID = resolvedCourtId,
            CategoryID = resolvedCategoryId,
            StatusID = defaultStatus.StatusID,
            StageID = defaultStage.StageID,
            Priority = request.Priority,
            SubjectMatter = request.SubjectMatter,
            FilingDate = request.FilingDate,
            InstitutionDate = request.InstitutionDate,
            RegistrationDate = request.RegistrationDate,
            ExpectedDisposalDate = request.ExpectedDisposalDate,
            ClaimedAmount = request.ClaimedAmount,
            PotentialLiability = request.PotentialLiability,
            FinancialImplication = request.FinancialImplication,
            ResponsibleDepartmentID = resolvedDepartmentId,
            CurrentLegalOfficerID = request.CurrentLegalOfficerID,
            CreatedBy = currentUserId,
            CreatedDate = DateTime.UtcNow,
            IsArchived = false
        };

        _context.Cases.Add(newCase);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Case created: {CaseID} with RefNo: {InternalRefNo}",newCase.CaseID, internalRefNo);

        // 10. Initial Status History
        var statusHistory = new CaseStatusHistory
        {
            CaseID = newCase.CaseID,
            OldStatusID = null,
            NewStatusID = defaultStatus.StatusID,
            ChangedBy = currentUserId,
            ChangedDate = DateTime.UtcNow,
            Remarks = "Case create ho gaya"
        };

        _context.CaseStatusHistories.Add(statusHistory);

        // 11. Audit Log
        var auditLog = _auditService.Create(currentUserId, $"Case Create: {newCase.CaseNumber}");
        _context.AuditLogs.Add(auditLog);

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Case successfully created with ID: {CaseID}", newCase.CaseID);

        return newCase.CaseID;
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

    // Type-or-select helpers for Court / Category / Department.
    // The Create Case form lets the user either pick an existing
    // option or type a brand-new one. If an ID was picked we validate
    // it exists (as before). If a name was typed instead, we reuse a
    // matching existing record (case-insensitive) if one exists for
    // this firm/global scope, otherwise we create a new firm-owned
    // record on the fly so it's available for every future case too.

    private async Task<int> GetOrCreateCourtIdAsync(int courtId, string? courtName, int firmId, CancellationToken cancellationToken)
    {
        if (courtId > 0)
        {
            var court = await _context.Courts.AsNoTracking().FirstOrDefaultAsync(x => x.CourtID == courtId && x.IsActive, cancellationToken);

            if (court == null)
            {
                _logger.LogWarning("Court not found or is inactive: {CourtID}", courtId);
                throw new NotFoundException($"Court ID {courtId} not found or is inactive");
            }
            return court.CourtID;
        }

        var name = courtName!.Trim();
        var existing = await _context.Courts.FirstOrDefaultAsync(x => x.IsActive && x.CourtName.ToLower() == name.ToLower(), cancellationToken);
        if (existing != null)
        {
            return existing.CourtID;
        }

        _logger.LogInformation("Creating new Court on the fly from Case creation: {CourtName}", name);
        var newCourt = new Models.Masters.Court
        {
            FirmID = firmId,
            CourtName = name,
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };
        _context.Courts.Add(newCourt);
        await _context.SaveChangesAsync(cancellationToken);
        return newCourt.CourtID;
    }

    private async Task<int> GetOrCreateCategoryIdAsync(int categoryId, string? categoryName, int firmId, CancellationToken cancellationToken)
    {
        if (categoryId > 0)
        {
            var category = await _context.CaseCategories.AsNoTracking().FirstOrDefaultAsync(x => x.CategoryID == categoryId, cancellationToken);

            if (category == null)
            {
                _logger.LogWarning("Category not found: {CategoryID}", categoryId);
                throw new NotFoundException($"Category ID {categoryId} not found");
            }
            return category.CategoryID;
        }

        var name = categoryName!.Trim();
        var existing = await _context.CaseCategories.FirstOrDefaultAsync(x => x.IsActive && x.CategoryName.ToLower() == name.ToLower(), cancellationToken);
        if (existing != null)
        {
            return existing.CategoryID;
        }

        _logger.LogInformation("Creating new Case Category on the fly from Case creation: {CategoryName}", name);
        var newCategory = new Models.Masters.CaseCategory
        {
            FirmID = firmId,
            CategoryName = name,
            IsActive = true
        };
        _context.CaseCategories.Add(newCategory);
        await _context.SaveChangesAsync(cancellationToken);
        return newCategory.CategoryID;
    }

    private async Task<int?> GetOrCreateDepartmentIdAsync(int? departmentId, string? departmentName, int firmId, CancellationToken cancellationToken)
    {
        if (departmentId.HasValue && departmentId.Value > 0)
        {
            var department = await _context.Departments.AsNoTracking().FirstOrDefaultAsync(x => x.DepartmentID == departmentId.Value && x.IsActive, cancellationToken);

            if (department == null)
            {
                _logger.LogWarning("Department not found ya inactive: {DepartmentID}", departmentId);
                throw new NotFoundException($"Department ID {departmentId} not found or is inactive");
            }
            return department.DepartmentID;
        }

        if (string.IsNullOrWhiteSpace(departmentName))
        {
            return null;
        }

        var name = departmentName.Trim();
        var existing = await _context.Departments.FirstOrDefaultAsync(x => x.IsActive && x.DepartmentName.ToLower() == name.ToLower(), cancellationToken);
        if (existing != null)
        {
            return existing.DepartmentID;
        }

        _logger.LogInformation("Creating new Department on the fly from Case creation: {DepartmentName}", name);
        var newDepartment = new Models.Masters.Department
        {
            FirmID = firmId,
            DepartmentName = name,
            IsActive = true
        };
        _context.Departments.Add(newDepartment);
        await _context.SaveChangesAsync(cancellationToken);
        return newDepartment.DepartmentID;
    }
}
