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









    public async Task<PagedResult<CaseDTO>> Handle(GetAllCasesQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching cases - Page: {PageNumber}, Size: {PageSize}",request.PageNumber,request.PageSize);


        var query = _context.Cases
            .AsNoTracking()
            .Include(x => x.Court)
            .Include(x => x.Category)
            .Include(x => x.Status)
            .Include(x => x.Stage)
            .Include(x => x.Department)
            .Include(x => x.LegalOfficer)
            .AsQueryable();



            query = query.Where(x => x.FirmID == _currentUser.FirmID);









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


        if (!string.IsNullOrWhiteSpace(request.SearchText))
        {
            var searchText = request.SearchText.Trim().ToLower();

            query = query.Where(x =>
                x.CaseNumber.ToLower().Contains(searchText) ||
                x.CaseTitle.ToLower().Contains(searchText) ||
                x.InternalReferenceNo.ToLower().Contains(searchText));

            _logger.LogInformation("Applied search filter: {SearchText}", searchText);
        }


        if (request.CourtID.HasValue && request.CourtID > 0)
        {
            query = query.Where(x => x.CourtID == request.CourtID.Value);
            _logger.LogInformation("Applied court filter: {CourtID}", request.CourtID);
        }


        if (request.StatusID.HasValue && request.StatusID > 0)
        {
            query = query.Where(x => x.StatusID == request.StatusID.Value);
            _logger.LogInformation("Applied status filter: {StatusID}", request.StatusID);
        }


        if (!string.IsNullOrWhiteSpace(request.Priority))
        {
            query = query.Where(x => x.Priority == request.Priority);
            _logger.LogInformation("Applied priority filter: {Priority}", request.Priority);
        }


        query = request.ArchivedOnly ? query.Where(x => x.IsArchived) : query.Where(x => !x.IsArchived);


        var totalRecords = await query.CountAsync(cancellationToken);




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
                ArchivedDate = x.ArchivedDate,
                ArchiveReason = x.ArchiveReason,
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
