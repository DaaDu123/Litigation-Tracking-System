using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using LTSBackend.Models.Masters;
using LTSBackend.Services.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.CaseStatuses.Commands.CreateCaseStatus;

public sealed class CreateCaseStatusHandler(AppDbContext _context, ICurrentUserService _currentUser, ILogger<CreateCaseStatusHandler> _logger) : IRequestHandler<CreateCaseStatusCommand, int>
{
    // =====================================================
    // HANDLE — creates a new case status
    // Trims input, checks the name is unique among what the caller can
    // see (global statuses + their own firm's), then saves it. FirmID is
    // set to the caller's own firm for a FirmAdmin, or left null (global,
    // visible to every firm) for a SuperAdmin.
    // =====================================================
    public async Task<int> Handle(CreateCaseStatusCommand request, CancellationToken cancellationToken)
    {
        request = request with { StatusName = request.StatusName.Trim(), ColorCode = request.ColorCode.Trim() };

        bool exists = await _context.CaseStatuses.AnyAsync(x => x.StatusName.ToLower() == request.StatusName.ToLower(), cancellationToken);
        if (exists)
        {
            _logger.LogWarning("Create failed: Case status already exists: {StatusName}", request.StatusName);
            throw new ValidationException(new() { $"Status '{request.StatusName}' already exists." });
        }

        var status = new CaseStatus
        {
            FirmID = _currentUser.FirmID,
            StatusName = request.StatusName,
            SequenceNo = request.SequenceNo,
            ColorCode = request.ColorCode,
            IsClosed = request.IsClosed,
            IsActive = request.IsActive
        };

        _context.CaseStatuses.Add(status);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Case status created successfully: {StatusID}", status.StatusID);

        return status.StatusID;
    }
}
