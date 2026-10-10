using LTSBackend.Features.Cases.DTOs;
using MediatR;

namespace LTSBackend.Features.Cases.Commands.CreateCase;

// Court and Category must be picked from the firm's existing master data
// (no more on-the-fly creation). Department is optional: when omitted it
// defaults from the category's Case Workflow Template.
public record CreateCaseCommand(
    string CaseNumber,
    string CaseTitle,
    string? CaseDescription,
    int CourtID,
    int CategoryID,
    string Priority,
    string SubjectMatter,
    DateTime FilingDate,
    DateTime InstitutionDate,
    DateTime RegistrationDate,
    DateTime? ExpectedDisposalDate,
    decimal ClaimedAmount,
    decimal PotentialLiability,
    string? FinancialImplication,
    int? ResponsibleDepartmentID,
    int? CurrentLegalOfficerID
) : IRequest<CreateCaseResultDTO>;
