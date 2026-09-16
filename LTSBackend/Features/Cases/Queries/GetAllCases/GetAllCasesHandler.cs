using LTSBackend.Comman.Responses;
using LTSBackend.Data;
using LTSBackend.Features.Cases.DTOs;
using LTSBackend.Features.Cases.Queries.GetAllCases;
using LTSBackend.Services.CurrentUser;
using LTSBackend.Services.Permissions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.Cases.Queries.GetAllCases;

public class GetAllCasesHandler(AppDbContext _context, ICurrentUserService _currentUser, IPermissionService _permissionService, ILogger<GetAllCasesHandler> _logger) : IRequestHandler<GetAllCasesQuery, PagedResult<CaseDTO>>
{
    // =====================================================
    // HANDLE — paged, filterable, role-scoped case list
    // Always firm-scoped. AssociateLawyer/Moharrir/InternParalegal (no
    // full case-directory visibility) are further narrowed to only cases
    // they're actively assigned to, applied at the query level (not
    // filtered in memory afterwards) so pagination/counts stay correct.
    // Supports search-text, court, status, and priority filters, and
    // excludes archived cases by default.
    // =====================================================
    public async Task<PagedResult<CaseDTO>> Handle(GetAllCasesQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching cases - Page: {PageNumber}, Size: {PageSize}",request.PageNumber,request.PageSize);

        // 1. Base query with relations
        var query = _context.Cases
            .AsNoTracking()
            .Include(x => x.Court)
            .Include(x => x.Category)
            .Include(x => x.Status)
            .Include(x => x.Stage)
            .Include(x => x.Department)
            .Include(x => x.LegalOfficer)
            .AsQueryable();

        // Multi-tenancy: firm-scoped. SuperAdmin cannot reach this endpoint at all
        // (route-level [Authorize] excludes it - case data is FirmAdmin's job).
            query = query.Where(x => x.FirmID == _currentUser.FirmID);

        // 1b. SECURITY FIX (BOLA): firm-scoping above is necessary but not
        // sufficient. Per the Roles & Permissions Matrix, only
        // SuperAdmin / FirmAdmin / Partner have "View Firm Case
        // Directory" rights — AssociateLawyer, Moharrir, and
        // InternParalegal must only see cases they are actively
        // assigned to. Previously this handler returned every case in
        // the firm to every role. We scope the query itself (rather
        // than filtering in memory) so pagination/counts stay correct.
        if (_currentUser.UserID.HasValue)
        {
            bool hasFullVisibility = await _permissionService.HasFullCaseDirectoryVisibilityAsync(_currentUser.UserID.Value, cancellationToken);

            if (!hasFullVisibility)
            {
                var userId = _currentUser.UserID.Value;
                var now = DateTime.UtcNow;

                query = query.Where(x => _context.CaseAssignments.Any(a =>
                    a.CaseID == x.CaseID &&
                    a.UserID == userId &&
                    (a.EndDate == null || a.EndDate > now)));
            }
        }

        // 2. Search filter - Case Number, Title, or Reference
        if (!string.IsNullOrWhiteSpace(request.SearchText))
        {
            var searchText = request.SearchText.Trim().ToLower();

            query = query.Where(x =>
                x.CaseNumber.ToLower().Contains(searchText) ||
                x.CaseTitle.ToLower().Contains(searchText) ||
                x.InternalReferenceNo.ToLower().Contains(searchText));

            _logger.LogInformation("Applied search filter: {SearchText}", searchText);
        }

        // 3. Filter by Court
        if (request.CourtID.HasValue && request.CourtID > 0)
        {
            query = query.Where(x => x.CourtID == request.CourtID.Value);
            _logger.LogInformation("Applied court filter: {CourtID}", request.CourtID);
        }

        // 4. Filter by Status
        if (request.StatusID.HasValue && request.StatusID > 0)
        {
            query = query.Where(x => x.StatusID == request.StatusID.Value);
            _logger.LogInformation("Applied status filter: {StatusID}", request.StatusID);
        }

        // 5. Filter by Priority
        if (!string.IsNullOrWhiteSpace(request.Priority))
        {
            query = query.Where(x => x.Priority == request.Priority);
            _logger.LogInformation("Applied priority filter: {Priority}", request.Priority);
        }

        // 6. Exclude archived cases by default
        query = query.Where(x => !x.IsArchived);

        // 7. Get total records count
        var totalRecords = await query.CountAsync(cancellationToken);

        // 8. Apply pagination
        // Department / LegalOfficer are nullable FKs (per schema), so
        // guard against NullReferenceException here.
        var cases = await query
            .OrderByDescending(x => x.CreatedDate)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(x => new CaseDTO
            {
                CaseID = x.CaseID,
                InternalReferenceNo = x.InternalReferenceNo,
                CaseNumber = x.CaseNumber,
                CaseTitle = x.CaseTitle,
                CaseDescription = x.CaseDescription,
                CourtID = x.CourtID,
                CourtName = x.Court.CourtName,
                CategoryID = x.CategoryID,
                CategoryName = x.Category.CategoryName,
                StatusID = x.StatusID,
                StatusName = x.Status.StatusName,
                StageID = x.StageID,
                StageName = x.Stage.StageName,
                DepartmentID = x.ResponsibleDepartmentID,
                DepartmentName = x.Department != null ? x.Department.DepartmentName : "Not Assigned",
                LegalOfficerID = x.CurrentLegalOfficerID,
                LegalOfficerName = x.LegalOfficer != null ? x.LegalOfficer.FullName : "Not Assigned",
                Priority = x.Priority,
                SubjectMatter = x.SubjectMatter,
                FilingDate = x.FilingDate,
                InstitutionDate = x.InstitutionDate,
                RegistrationDate = x.RegistrationDate,
                ExpectedDisposalDate = x.ExpectedDisposalDate,
                ClaimedAmount = x.ClaimedAmount,
                PotentialLiability = x.PotentialLiability,
                IsArchived = x.IsArchived,
                CreatedDate = x.CreatedDate
            })
            .ToListAsync(cancellationToken);

        _logger.LogInformation(
            "Fetched {Count} of {Total} total cases",
            cases.Count,
            totalRecords);

        return new PagedResult<CaseDTO>
        {
            Items = cases,
            TotalRecords = totalRecords,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}
