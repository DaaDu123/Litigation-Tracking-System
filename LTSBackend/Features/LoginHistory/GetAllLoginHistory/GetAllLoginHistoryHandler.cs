using LTSBackend.Comman.Responses;
using LTSBackend.Data;
using LTSBackend.Features.LoginHistory.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.LoginHistory.GetAllLoginHistory;

/// <summary>
/// Returns a paged, filterable list of login history records.
///
/// TENANT ISOLATION (fixed - see AppDbContext.cs): this query intentionally
/// does NOT add its own ".Where(x => x.User.FirmID == ...)" filter. Doing so
/// used to be necessary and was missing here, which meant a Firm Admin with
/// the ViewLoginHistory permission could see every firm's login IPs, emails
/// and timestamps - a full cross-tenant data leak. Isolation is now enforced
/// centrally by the global query filter on LoginHistory in AppDbContext, so
/// every query through this DbSet is automatically scoped to the caller's
/// own firm (or unrestricted for SuperAdmin). Do NOT call
/// ".IgnoreQueryFilters()" here - that would silently reopen the leak.
/// </summary>
public class GetAllLoginHistoryHandler(AppDbContext context) : IRequestHandler<GetAllLoginHistoryQuery, PagedResult<LoginHistoryDTO>>
{
    // =====================================================
    // HANDLE — paged, filterable login-history list (search/date range/status)
    // Tenant isolation is enforced centrally by AppDbContext's global
    // query filter on LoginHistory, not by this handler — see the
    // class-level note above.
    // =====================================================
    public async Task<PagedResult<LoginHistoryDTO>> Handle(GetAllLoginHistoryQuery request, CancellationToken cancellationToken)
    {
        var query = context.LoginHistories.AsNoTracking().Include(x => x.User).ThenInclude(u => u.Firm).AsQueryable();

        //----------------------------------------
        // Search
        //----------------------------------------

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            query = query.Where(x => x.User.FullName.Contains(search) || x.User.Email.Contains(search));
        }

        //----------------------------------------
        // Date Filter
        //----------------------------------------

        if (request.FromDate.HasValue)
        {
            query = query.Where(x => x.LoginTime >= request.FromDate.Value);
        }

        if (request.ToDate.HasValue)
        {
            query = query.Where(x => x.LoginTime <= request.ToDate.Value);
        }

        //----------------------------------------
        // Status Filter
        //----------------------------------------

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            query = query.Where(x => x.Status == request.Status);
        }

        //----------------------------------------
        // Total Records
        //----------------------------------------

        var totalRecords = await query.CountAsync(cancellationToken);

        //----------------------------------------
        // Data
        //    FIX: LoginHistory model has no CreatedDate column - the
        //    record is created at login time, so LoginTime is the
        //    closest equivalent. Mapped here instead of a non-existent
        //    x.CreatedDate (which was a compile error: 'LoginHistory'
        //    does not contain a definition for 'CreatedDate').
        //----------------------------------------

        var items = await query
            .OrderByDescending(x => x.LoginTime)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(x => new LoginHistoryDTO
            {
                LoginID = x.LoginID,
                UserID = x.UserID,
                FullName = x.User.FullName,
                Email = x.User.Email,
                FirmName = x.User.Firm != null ? x.User.Firm.FirmName : null,
                LoginTime = x.LoginTime,
                LogoutTime = x.LogoutTime,
                IPAddress = x.IPAddress,
                UserAgent = x.UserAgent,
                Status = x.Status,
                IsLoggedOut = x.IsLoggedOut,
                CreatedDate = x.LoginTime
            }).ToListAsync(cancellationToken);

        return new PagedResult<LoginHistoryDTO>
        {
            Items = items,
            TotalRecords = totalRecords,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}