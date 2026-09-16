using LTSBackend.Features.Cases.DTOs;
using MediatR;

namespace LTSBackend.Features.Cases.Commands.UpdateCase;

public record UpdateCaseCommand(
    long CaseID,
    string? CaseNumber,
    string? CaseTitle,
    string? CaseDescription,
    int? CourtID,
    int? CategoryID,
    int? StageID,
    string? Priority,
    string? SubjectMatter,
    DateTime? ExpectedDisposalDate,
    decimal? ClaimedAmount,
    decimal? PotentialLiability,
    int? CurrentLegalOfficerID,
    bool? IsArchived
) : IRequest<bool>;