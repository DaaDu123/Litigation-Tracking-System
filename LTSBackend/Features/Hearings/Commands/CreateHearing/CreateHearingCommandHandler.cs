using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using LTSBackend.Data;
using LTSBackend.Models.Cases;
using LTSBackend.Comman.Exceptions;
using LTSBackend.Services.CurrentUser;
using LTSBackend.Services.Permissions;

namespace LTSBackend.Features.Hearings.Commands.CreateHearing
{
    public class CreateHearingCommandHandler : IRequestHandler<CreateHearingCommand, long>
    {
        private readonly AppDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly IPermissionService _permissionService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CreateHearingCommandHandler(AppDbContext context, ICurrentUserService currentUser, IPermissionService permissionService, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _currentUser = currentUser;
            _permissionService = permissionService;
            _httpContextAccessor = httpContextAccessor;
        }










        public async Task<long> Handle(CreateHearingCommand request, CancellationToken cancellationToken)
        {

            var caseEntity = await _context.Cases.FirstOrDefaultAsync(c => c.CaseID == request.Hearing.CaseId, cancellationToken);
            if (caseEntity == null || (caseEntity.FirmID != _currentUser.FirmID))
                throw new NotFoundException("Case not found");




            if (_currentUser.UserID.HasValue)
            {
                bool hasFullVisibility = await _permissionService.HasFullCaseDirectoryVisibilityAsync(_currentUser.UserID.Value, cancellationToken);
                if (!hasFullVisibility)
                {
                    bool isAssignedToCase = await _permissionService.IsUserAssignedToCaseAsync(_currentUser.UserID.Value, request.Hearing.CaseId, cancellationToken);
                    if (!isAssignedToCase)
                        throw new NotFoundException("Case not found");
                }
            }


            var courtExists = await _context.Courts.AnyAsync(c => c.CourtID == request.Hearing.CourtId, cancellationToken);
            if (!courtExists)
                throw new NotFoundException("Court not found");


            var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim?.Value, out int userId))
                throw new UnauthorizedException("User not authenticated");


            var hearing = new Hearing
            {
                CaseID = request.Hearing.CaseId,
                CourtID = request.Hearing.CourtId,
                HearingDate = request.Hearing.HearingDate,
                CourtRoom = request.Hearing.CourtRoom,
                JudgeName = request.Hearing.JudgeName,
                Purpose = request.Hearing.HearingPurpose,
                Remarks = request.Hearing.Remarks,
                CreatedBy = userId,
                CreatedDate = DateTime.UtcNow
            };

            _context.Hearings.Add(hearing);
            await _context.SaveChangesAsync(cancellationToken);

            return hearing.HearingID;
        }
    }
}