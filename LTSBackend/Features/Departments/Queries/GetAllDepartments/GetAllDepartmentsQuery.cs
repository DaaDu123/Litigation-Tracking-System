using LTSBackend.Features.Departments.DTOs;
using MediatR;

namespace LTSBackend.Features.Departments.Queries.GetAllDepartments;

/// <summary>
/// Returns all departments. Set ActiveOnly = true to filter out
/// deactivated departments (useful for populating dropdowns).
/// </summary>
public sealed record GetAllDepartmentsQuery(bool ActiveOnly = false) : IRequest<List<DepartmentDTO>>;
