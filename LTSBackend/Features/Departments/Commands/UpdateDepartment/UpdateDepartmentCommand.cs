using MediatR;

namespace LTSBackend.Features.Departments.Commands.UpdateDepartment;

public sealed record UpdateDepartmentCommand(int DepartmentID, string DepartmentName, string? DepartmentCode, string? Description, bool IsActive) : IRequest<bool>;
