using LTSBackend.Features.Departments.DTOs;
using MediatR;

namespace LTSBackend.Features.Departments.Queries.GetAllDepartments;





public sealed record GetAllDepartmentsQuery(bool ActiveOnly = false) : IRequest<List<DepartmentDTO>>;
