using MediatR;

namespace LTSBackend.Features.Departments.Commands.DeleteDepartment;

public sealed record DeleteDepartmentCommand(int DepartmentID) : IRequest<bool>;
