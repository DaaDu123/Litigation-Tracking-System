using LTSBackend.Comman.Responses;
using LTSBackend.Features.DocumentTypes.Commands.CreateDocumentType;
using LTSBackend.Features.DocumentTypes.Commands.DeleteDocumentType;
using LTSBackend.Features.DocumentTypes.Commands.UpdateDocumentType;
using LTSBackend.Features.DocumentTypes.DTOs;
using LTSBackend.Features.DocumentTypes.Queries.GetAllDocumentTypes;
using LTSBackend.Features.DocumentTypes.Queries.GetDocumentTypeById;
using LTSBackend.Features.DocumentTypes.Queries.GetDocumentTypeOptions;
using LTSBackend.Models.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LTSBackend.Features.DocumentTypes.Controllers;

/// <summary>
/// Master data for document types (e.g. Petition, Affidavit, Court Order).
/// Same per-tenant model as Courts/Departments/CaseCategories/CaseStages -
/// see CreateDocumentTypeHandler/UpdateDocumentTypeHandler/
/// DeleteDocumentTypeHandler for the ownership rules.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class DocumentTypesController(IMediator mediator) : ControllerBase
{
    // =====================================================
    // GET DOCUMENT TYPE OPTIONS (dropdown) — same roles as Document upload
    // Deliberately NOT a master-data admin endpoint: returns only
    // {DocumentTypeID, TypeName} pairs, nothing else, and is reachable by
    // every role that DocumentsController.UploadDocument allows
    // (Partner/AssociateLawyer/Moharrir/InternParalegal/FirmAdmin), not
    // just FirmAdmin/Partner. This is what closes the gap: those roles can
    // upload a document but cannot reach GetAll below (master-data admin
    // is FirmAdmin/Partner only) - this endpoint gives them just enough to
    // populate the "Document Type" dropdown on the upload form, without
    // granting any create/update/delete/admin-list access.
    // =====================================================
    [HttpGet("options")]
    [Authorize(Roles = RoleNames.AllFirmUsers)]
    public async Task<IActionResult> GetOptions()
    {
        var options = await mediator.Send(new GetDocumentTypeOptionsQuery());
        return Ok(ApiResponse<List<DocumentTypeOptionDTO>>.SuccessResponse(options));
    }

    // =====================================================
    // GET ALL DOCUMENT TYPES — FirmAdmin and above ONLY
    // Master-data management is FirmAdmin and Partner's task.
    // AssociateLawyer, Moharrir, InternParalegal and SuperAdmin have NO
    // access (not even read) to this master data admin surface. For the
    // Document upload dropdown, AssociateLawyer/Moharrir/InternParalegal
    // use the "options" endpoint above instead.
    // =====================================================
    [HttpGet]
    [Authorize(Roles = RoleNames.FirmAdminAndAbove)]
    public async Task<IActionResult> GetAll([FromQuery] string? searchText, [FromQuery] bool activeOnly = true)
    {
        var types = await mediator.Send(new GetAllDocumentTypesQuery(searchText, activeOnly));
        return Ok(ApiResponse<List<DocumentTypeDTO>>.SuccessResponse(types));
    }

    // =====================================================
    // GET DOCUMENT TYPE BY ID — FirmAdmin and above ONLY
    // Fetches a single document type's details by its ID.
    // =====================================================
    [HttpGet("{id}")]
    [Authorize(Roles = RoleNames.FirmAdminAndAbove)]
    public async Task<IActionResult> GetById(int id)
    {
        var type = await mediator.Send(new GetDocumentTypeByIdQuery(id));
        return Ok(ApiResponse<DocumentTypeDTO>.SuccessResponse(type));
    }

    // =====================================================
    // CREATE DOCUMENT TYPE — FirmAdmin and above
    // Adds a new document type. A FirmAdmin's new type is scoped to their
    // own firm; only a SuperAdmin can create a global type.
    // =====================================================
    [HttpPost]
    [Authorize(Roles = RoleNames.FirmAdminAndAbove)]
    public async Task<IActionResult> Create(CreateDocumentTypeCommand command)
    {
        var id = await mediator.Send(command);
        return Ok(ApiResponse<int>.SuccessResponse(id, "Document type created successfully."));
    }

    // =====================================================
    // UPDATE DOCUMENT TYPE — FirmAdmin and above
    // Edits an existing document type's name/description/active flag.
    // Route id and body DocumentTypeID must match.
    // =====================================================
    [HttpPut("{id}")]
    [Authorize(Roles = RoleNames.FirmAdminAndAbove)]
    public async Task<IActionResult> Update(int id, UpdateDocumentTypeCommand command)
    {
        if (id != command.DocumentTypeID)
            return BadRequest(ApiResponse<bool>.FailureResponse("Route ID and body DocumentTypeID do not match."));

        var result = await mediator.Send(command);
        return Ok(ApiResponse<bool>.SuccessResponse(result, "Document type updated successfully."));
    }

    // =====================================================
    // DELETE DOCUMENT TYPE — FirmAdmin and above
    // Removes a document type the firm no longer uses. Ownership/in-use
    // checks are enforced in the handler.
    // =====================================================
    [HttpDelete("{id}")]
    [Authorize(Roles = RoleNames.FirmAdminAndAbove)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await mediator.Send(new DeleteDocumentTypeCommand(id));
        return Ok(ApiResponse<bool>.SuccessResponse(result, "Document type deleted successfully."));
    }
}
