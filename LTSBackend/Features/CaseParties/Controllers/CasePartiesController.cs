using LTSBackend.Comman.Responses;
using LTSBackend.Features.CaseParties.Commands.CreateCaseParty;
using LTSBackend.Features.CaseParties.Commands.DeleteCaseParty;
using LTSBackend.Features.CaseParties.Commands.UpdateCaseParty;
using LTSBackend.Features.CaseParties.DTOs;
using LTSBackend.Features.CaseParties.Queries.GetCaseParties;
using LTSBackend.Features.CaseParties.Queries.GetCasePartyById;
using LTSBackend.Models.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LTSBackend.Features.CaseParties.Controllers;

/// <summary>
/// SRS Reference: Complete Database Schema - CaseParties table
/// Litigation_Tracking_System_Case_SRS.docx Section 5.4 "Party Information"
/// Plaintiff / Petitioner / Defendant / Respondent management per case
/// </summary>
[Route("api/cases/{caseId}/parties")]
[ApiController]
[Authorize]
public class CasePartiesController(IMediator _mediator) : ControllerBase
{
    // =====================================================
    // GET PARTIES FOR A CASE — Any firm user
    // Lists every party (plaintiff, defendant, petitioner, respondent,
    // etc.) recorded against the given case.
    // =====================================================
    [HttpGet]
    [Authorize(Roles = RoleNames.AllFirmUsers)]
    public async Task<IActionResult> GetByCase(long caseId)
    {
        var result = await _mediator.Send(new GetCasePartiesQuery { CaseID = caseId });
        return Ok(ApiResponse<List<CasePartyDetailDTO>>.SuccessResponse(result, "Case parties fetched"));
    }

    // =====================================================
    // GET PARTY BY ID — Any firm user
    // Fetches a single party's full detail record within a case.
    // =====================================================
    [HttpGet("{partyId}")]
    [Authorize(Roles = RoleNames.AllFirmUsers)]
    public async Task<IActionResult> GetById(long caseId, long partyId)
    {
        var result = await _mediator.Send(new GetCasePartyByIdQuery { PartyID = partyId });
        return Ok(ApiResponse<CasePartyDetailDTO>.SuccessResponse(result, "Party fetched"));
    }

    // =====================================================
    // CREATE PARTY — Lawyer roles
    // Adds a new party (plaintiff/defendant/petitioner/respondent, etc.)
    // to the given case. CaseID is taken from the route, not trusted from
    // the request body.
    // =====================================================
    [HttpPost]
    [Authorize(Roles = RoleNames.AllLawyers)]
    public async Task<IActionResult> Create(long caseId, [FromBody] CreateCasePartyDTO dto)
    {
        dto.CaseID = caseId;
        var partyId = await _mediator.Send(new CreateCasePartyCommand { Party = dto });
        return CreatedAtAction(nameof(GetById), new { caseId, partyId }, ApiResponse<long>.SuccessResponse(partyId, "Party successfully added"));
    }

    // =====================================================
    // UPDATE PARTY — Lawyer roles
    // Edits an existing party's details (name, contact info, lawyer name,
    // etc.).
    // =====================================================
    [HttpPut("{partyId}")]
    [Authorize(Roles = RoleNames.AllLawyers)]
    public async Task<IActionResult> Update(long caseId, long partyId, [FromBody] UpdateCasePartyDTO dto)
    {
        dto.PartyID = partyId;
        var result = await _mediator.Send(new UpdateCasePartyCommand { Party = dto });
        return Ok(ApiResponse<bool>.SuccessResponse(result, "Party successfully updated"));
    }

    // =====================================================
    // DELETE PARTY — Partner and above
    // Removes a party from a case. Restricted to Partner-and-above since
    // removing a party (e.g. a defendant) is a more consequential edit
    // than adding/updating one.
    // =====================================================
    [HttpDelete("{partyId}")]
    [Authorize(Roles = RoleNames.PartnerAndAbove)]
    public async Task<IActionResult> Delete(long caseId, long partyId)
    {
        var result = await _mediator.Send(new DeleteCasePartyCommand { PartyID = partyId });
        return Ok(ApiResponse<bool>.SuccessResponse(result, "Party successfully deleted"));
    }
}
