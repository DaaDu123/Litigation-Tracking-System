using LTSBackend.Features.Cases.DTOs;
using MediatR;

namespace LTSBackend.Features.Cases.Commands.CreateCase;




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
