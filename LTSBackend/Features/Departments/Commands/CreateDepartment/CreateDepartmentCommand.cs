using MediatR;

namespace LTSBackend.Features.Departments.Commands.CreateDepartment;

public sealed record CreateDepartmentCommand(string DepartmentName, string? DepartmentCode, string? Description, bool IsActive = true) : IRequest<int>;
