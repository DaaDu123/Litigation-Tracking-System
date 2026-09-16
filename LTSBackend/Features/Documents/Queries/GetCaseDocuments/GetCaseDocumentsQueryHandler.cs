using LTSBackend.Comman.Enum;
using LTSBackend.Data;
using LTSBackend.Features.Documents.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.Documents.Queries.GetCaseDocuments
{
    public class GetCaseDocumentsQueryHandler(AppDbContext _context) : IRequestHandler<GetCaseDocumentsQuery, List<DocumentDetailDTO>>
    {
        public async Task<List<DocumentDetailDTO>> Handle(GetCaseDocumentsQuery request, CancellationToken cancellationToken)
        {
            var caseInfo = await _context.Cases
                .AsNoTracking()
                .Where(c => c.CaseID == request.CaseID)
                .Select(c => new { c.FirmID, c.CaseNumber })
                .FirstOrDefaultAsync(cancellationToken);

            var caseFirmId = caseInfo?.FirmID;
            var user = await _context.Users
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Include(u => u.Role)
                .Include(u => u.Firm)
                .FirstOrDefaultAsync(u => u.UserID == request.UserID, cancellationToken);

            if (caseFirmId == null || user == null || user.IsDeleted || !user.IsActive)
                return new List<DocumentDetailDTO>();

            if (user.Firm != null && (user.Firm.IsBlocked || user.Firm.IsDeleted))
                return new List<DocumentDetailDTO>();

            var role = user.GetRole();

            bool isSuperAdmin = role == UserRole.SuperAdmin;
            if (!isSuperAdmin && caseFirmId != user.FirmID)
                return new List<DocumentDetailDTO>();

            bool fullFirmAccess = isSuperAdmin || role == UserRole.FirmAdmin || role == UserRole.Partner;

            bool isAssignedToCase = false;
            if (!fullFirmAccess && role is UserRole.AssociateLawyer or UserRole.InternParalegal or UserRole.Moharrir)
            {
                var now = DateTime.UtcNow;
                isAssignedToCase = await _context.CaseAssignments
                    .AsNoTracking()
                    .AnyAsync(a => a.CaseID == request.CaseID && a.UserID == request.UserID && (a.EndDate == null || a.EndDate > now), cancellationToken);
            }

            if (!fullFirmAccess && role is UserRole.AssociateLawyer or UserRole.InternParalegal && !isAssignedToCase)
                return new List<DocumentDetailDTO>();

            if (role == UserRole.Moharrir && !isAssignedToCase)
                return new List<DocumentDetailDTO>();

            if (!fullFirmAccess && role != UserRole.AssociateLawyer && role != UserRole.InternParalegal && role != UserRole.Moharrir)
                return new List<DocumentDetailDTO>();

            var documents = await _context.Documents
                .AsNoTracking()
                .Include(d => d.DocumentType)
                .Where(d => d.CaseID == request.CaseID && d.IsLatest)
                .OrderByDescending(d => d.UploadedDate)
                .ToListAsync(cancellationToken);

            if (documents.Count == 0)
                return new List<DocumentDetailDTO>();

            HashSet<long>? moharrirVisibleDocIds = null;
            if (role == UserRole.Moharrir)
            {
                var documentIds = documents.Select(d => d.DocumentID).ToList();

                var overrides = await _context.DocumentPermissions
                    .AsNoTracking()
                    .Where(p => documentIds.Contains(p.DocumentID) && (p.UserID == request.UserID || p.RoleID == user.RoleID))
                    .ToListAsync(cancellationToken);

                bool isElevatedByDefault = user.Role != null && await _context.Users
                    .AsNoTracking()
                    .Where(u => u.UserID == request.UserID)
                    .Include(u => u.Role!).ThenInclude(r => r.RolePermissions).ThenInclude(rp => rp.Permission)
                    .AnyAsync(u => u.Role!.RolePermissions.Any(rp => rp.Permission!.PermissionName == "ViewDocumentsIfPermitted"), cancellationToken);

                moharrirVisibleDocIds = new HashSet<long>();
                foreach (var docId in documentIds)
                {
                    var userOverride = overrides.FirstOrDefault(p => p.DocumentID == docId && p.UserID == request.UserID);
                    var roleOverride = overrides.FirstOrDefault(p => p.DocumentID == docId && p.RoleID == user.RoleID && p.UserID == null);

                    bool canView = userOverride != null ? userOverride.CanView
                        : roleOverride != null ? roleOverride.CanView
                        : isElevatedByDefault;

                    if (canView) moharrirVisibleDocIds.Add(docId);
                }
            }

            var uploaderIds = documents.Select(d => d.UploadedBy).Distinct().ToList();
            var approverIds = documents.Where(d => d.ApprovedBy.HasValue).Select(d => d.ApprovedBy!.Value).Distinct().ToList();
            var allNameIds = uploaderIds.Union(approverIds).ToList();
            var uploaderNames = await _context.Users
                .AsNoTracking()
                .Where(u => allNameIds.Contains(u.UserID))
                .ToDictionaryAsync(u => u.UserID, u => u.FullName, cancellationToken);

            var caseNumber = caseInfo?.CaseNumber ?? "Unknown";

            bool canSeeDrafts = role == UserRole.FirmAdmin || role == UserRole.Partner;

            var result = new List<DocumentDetailDTO>();
            foreach (var document in documents)
            {
                if (moharrirVisibleDocIds != null && !moharrirVisibleDocIds.Contains(document.DocumentID))
                    continue;

                if (document.IsDraft && document.UploadedBy != request.UserID && !canSeeDrafts)
                    continue;

                result.Add(new DocumentDetailDTO
                {
                    DocumentID = document.DocumentID,
                    CaseID = document.CaseID,
                    CaseNumber = caseNumber,
                    DocumentName = document.DocumentName,
                    FileName = document.FileName,
                    DocumentType = document.DocumentType?.TypeName ?? "Unknown",
                    FileSize = document.FileSize,
                    VersionNo = document.VersionNo,
                    IsLatest = document.IsLatest,
                    UploadedBy = uploaderNames.TryGetValue(document.UploadedBy, out var name) ? name : "Unknown User",
                    UploadedDate = document.UploadedDate,
                    Remarks = document.Remarks ?? string.Empty,
                    IsDraft = document.IsDraft,
                    ApprovedByName = document.ApprovedBy.HasValue && uploaderNames.TryGetValue(document.ApprovedBy.Value, out var approverName) ? approverName : null,
                    ApprovedDate = document.ApprovedDate
                });
            }

            return result;
        }
    }
}