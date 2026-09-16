using LTSFrontend.Features.CaseAssignments.DTOs;

namespace LTSFrontend.Features.CaseAssignments.Services
{
    public interface ICaseAssignmentService
    {
        Task<List<CaseAssignmentDTO>> GetByCaseAsync(long caseId, bool activeOnly = false);
        Task<List<CaseAssignmentDTO>> GetMyAssignedCasesAsync();
        Task<long> AssignAsync(AssignCaseDTO dto);
        Task<bool> UpdateAsync(UpdateAssignmentDTO dto);
        Task<bool> EndAsync(long assignmentId, string? remarks);
    }
}
