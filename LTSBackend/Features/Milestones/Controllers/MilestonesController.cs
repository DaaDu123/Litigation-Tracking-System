using LTSBackend.Comman.Responses;
using LTSBackend.Features.Milestones.Commands.CompleteMilestone;
using LTSBackend.Features.Milestones.Commands.CreateMilestone;
using LTSBackend.Features.Milestones.Commands.DeleteMilestone;
using LTSBackend.Features.Milestones.Commands.UpdateMilestone;
using LTSBackend.Features.Milestones.DTOs;
using LTSBackend.Features.Milestones.Queries.GetCaseMilestones;
using LTSBackend.Models.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LTSBackend.Features.Milestones.Controllers;






[Route("api/[controller]")]
[ApiController]
[Authorize]
public class MilestonesController(IMediator _mediator) : ControllerBase
{




    [HttpGet("case/{caseId}")]
    [Authorize(Roles = RoleNames.AllFirmUsers)]
    public async Task<IActionResult> GetByCase(long caseId)
    {
        var result = await _mediator.Send(new GetCaseMilestonesQuery { CaseID = caseId });
        return Ok(ApiResponse<List<MilestoneDetailDTO>>.SuccessResponse(result, "Case milestones fetched"));
    }





    [HttpPost]
    [Authorize(Roles = RoleNames.AllLawyers)]
    public async Task<IActionResult> Create([FromBody] CreateMilestoneDTO dto)
    {
        var id = await _mediator.Send(new CreateMilestoneCommand { Milestone = dto });
        return CreatedAtAction(nameof(GetByCase), new { caseId = dto.CaseID }, ApiResponse<long>.SuccessResponse(id, "Milestone successfully created"));
    }





    [HttpPut("{id}")]
    [Authorize(Roles = RoleNames.AllLawyers)]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateMilestoneDTO dto)
    {
        dto.MilestoneID = id;
        var result = await _mediator.Send(new UpdateMilestoneCommand { Milestone = dto });
        return Ok(ApiResponse<bool>.SuccessResponse(result, "Milestone successfully updated"));
    }





    [HttpPut("{id}/complete")]
    [Authorize(Roles = RoleNames.AllLawyers)]
    public async Task<IActionResult> Complete(long id)
    {
        var result = await _mediator.Send(new CompleteMilestoneCommand { MilestoneID = id });
        return Ok(ApiResponse<bool>.SuccessResponse(result, "Milestone marked complete"));
    }







    [HttpDelete("{id}")]
    [Authorize(Roles = RoleNames.PartnerAndAbove)]
    public async Task<IActionResult> Delete(long id)
    {
        var result = await _mediator.Send(new DeleteMilestoneCommand { MilestoneID = id });
        return Ok(ApiResponse<bool>.SuccessResponse(result, "Milestone successfully deleted"));
    }
}
