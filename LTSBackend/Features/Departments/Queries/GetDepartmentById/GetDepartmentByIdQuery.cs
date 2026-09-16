using LTSBackend.Features.Departments.DTOs;
using MediatR;

namespace LTSBackend.Features.Departments.Queries.GetDepartmentById;

public sealed record GetDepartmentByIdQuery(int DepartmentID) : IRequest<DepartmentDTO>;
