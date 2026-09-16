using MediatR;

namespace LTSBackend.Features.Cases.Commands.CreateCase;

public record CreateCaseCommand(
    string CaseNumber,
    string CaseTitle,
    string? CaseDescription,
    int CourtID,
    string? CourtName,
    int CategoryID,
    string? CategoryName,
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
    string? DepartmentName,
    int? CurrentLegalOfficerID
) : IRequest<long>;
