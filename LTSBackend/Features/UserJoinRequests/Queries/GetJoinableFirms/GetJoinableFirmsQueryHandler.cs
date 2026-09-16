using LTSBackend.Data;
using LTSBackend.Features.UserJoinRequests.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.UserJoinRequests.Queries.GetJoinableFirms;

public class GetJoinableFirmsQueryHandler(AppDbContext _context) : IRequestHandler<GetJoinableFirmsQuery, List<JoinableFirmDTO>>
{
    // =====================================================
    // HANDLE — public firm picker for the join-request form
    // [AllowAnonymous] on the controller, so this deliberately returns
    // only FirmID/FirmName/FirmCode - nothing internal - and only for
    // firms that are actually usable right now (not blocked, not removed).
    // =====================================================
    public async Task<List<JoinableFirmDTO>> Handle(GetJoinableFirmsQuery request, CancellationToken cancellationToken)
    {
        return await _context.Firms.AsNoTracking()
            .Where(x => !x.IsDeleted && !x.IsBlocked)
            .OrderBy(x => x.FirmName)
            .Select(x => new JoinableFirmDTO
            {
                FirmID = x.FirmID,
                FirmName = x.FirmName,
                FirmCode = x.FirmCode
            })
            .ToListAsync(cancellationToken);
    }
}
