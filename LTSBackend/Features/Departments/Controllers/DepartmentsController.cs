using LTSBackend.Comman.Responses;
using LTSBackend.Features.Departments.Commands.CreateDepartment;
using LTSBackend.Features.Departments.Commands.DeleteDepartment;
using LTSBackend.Features.Departments.Commands.UpdateDepartment;
using LTSBackend.Features.Departments.DTOs;
using LTSBackend.Features.Departments.Queries.GetAllDepartments;
using LTSBackend.Features.Departments.Queries.GetDepartmentById;
using LTSBackend.Models.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LTSBackend.Features.Departments.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class DepartmentsController(IMediator mediator) : ControllerBase
{









    [HttpGet]
    [Authorize(Roles = RoleNames.FirmAdminAndAbove)]
    public async Task<IActionResult> GetAll([FromQuery] bool activeOnly = false)
    {
        var departments = await mediator.Send(new GetAllDepartmentsQuery(activeOnly));
        return Ok(ApiResponse<List<DepartmentDTO>>.SuccessResponse(departments));
    }





    [HttpGet("{id}")]
    [Authorize(Roles = RoleNames.FirmAdminAndAbove)]
    public async Task<IActionResult> GetById(int id)
    {
        var department = await mediator.Send(new GetDepartmentByIdQuery(id));
        return Ok(ApiResponse<DepartmentDTO>.SuccessResponse(department));
    }









    [HttpPost]
    [Authorize(Roles = RoleNames.FirmAdminAndAbove)]
    public async Task<IActionResult> Create(CreateDepartmentCommand command)
    {
        var id = await mediator.Send(command);
        return Ok(ApiResponse<int>.SuccessResponse(id, "Department created successfully."));
    }







    [HttpPut("{id}")]
    [Authorize(Roles = RoleNames.FirmAdminAndAbove)]
    public async Task<IActionResult> Update(int id, UpdateDepartmentCommand command)
    {
        if (id != command.DepartmentID)
            return BadRequest(ApiResponse<bool>.FailureResponse("Route ID and body DepartmentID do not match."));

        var result = await mediator.Send(command);
        return Ok(ApiResponse<bool>.SuccessResponse(result, "Department updated successfully."));
    }








    [HttpDelete("{id}")]
    [Authorize(Roles = RoleNames.FirmAdminAndAbove)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await mediator.Send(new DeleteDepartmentCommand(id));
        return Ok(ApiResponse<bool>.SuccessResponse(result, "Department deleted successfully."));
    }
}
