using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LTSBackend.Comman.Responses;
using LTSBackend.Features.Hearings.Commands.CreateHearing;
using LTSBackend.Features.Hearings.Commands.DeleteHearing;
using LTSBackend.Features.Hearings.Commands.UpdateHearing;
using LTSBackend.Features.Hearings.DTOs;
using LTSBackend.Features.Hearings.Queries.GetCaseHearings;
using LTSBackend.Features.Hearings.Queries.GetHearingById;
using LTSBackend.Features.Hearings.Queries.GetUpcomingHearings;
using LTSBackend.Features.Hearings.Commands.RecordAttendance;
using LTSBackend.Features.Hearings.Commands.UpdateAttendance;
using LTSBackend.Features.Hearings.Commands.DeleteAttendance;
using LTSBackend.Features.Hearings.Queries.GetHearingAttendance;
using LTSBackend.Models.Security;

namespace LTSBackend.Features.Hearings.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class HearingsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public HearingsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // =====================================================
        // CREATE HEARING — Lawyer roles
        // Schedules a new hearing against a case (date, court, purpose,
        // etc.). Past-date scheduling is rejected by CreateHearingValidator
        // before this ever reaches the handler.
        // =====================================================
        [HttpPost]
        [Authorize(Roles = RoleNames.AllLawyers)]
        [ProducesResponseType(typeof(ApiResponse<long>), 201)]
        [ProducesResponseType(typeof(ApiResponse<string>), 400)]
        public async Task<IActionResult> CreateHearing([FromBody] CreateHearingDTO dto)
        {
            var command = new CreateHearingCommand { Hearing = dto };
            var hearingId = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetHearingById), new { id = hearingId },
                new ApiResponse<long> { Success = true, Data = hearingId, Message = "Hearing created successfully" });
        }

        // =====================================================
        // GET HEARING BY ID — Any firm user
        // Fetches a single hearing's full details.
        // =====================================================
        [HttpGet("{id}")]
        [Authorize(Roles = RoleNames.AllFirmUsers)]
        [ProducesResponseType(typeof(ApiResponse<HearingDetailDTO>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 404)]
        public async Task<IActionResult> GetHearingById(long id)
        {
            var query = new GetHearingByIdQuery { HearingId = id };
            var result = await _mediator.Send(query);
            return Ok(new ApiResponse<HearingDetailDTO> { Success = true, Data = result });
        }

        // =====================================================
        // GET UPCOMING HEARINGS — Any firm user
        // Returns a paged list of future hearings across the caller's
        // visible cases, optionally filtered by case or court, for the
        // "upcoming hearings" dashboard widget.
        // =====================================================
        [HttpGet("upcoming")]
        [Authorize(Roles = RoleNames.AllFirmUsers)]
        [ProducesResponseType(typeof(ApiResponse<PagedHearingResult<HearingDetailDTO>>), 200)]
        public async Task<IActionResult> GetUpcomingHearings(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] long? caseId = null,
            [FromQuery] int? courtId = null)
        {
            var query = new GetUpcomingHearingsQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                CaseId = caseId,
                CourtId = courtId
            };
            var result = await _mediator.Send(query);
            return Ok(new ApiResponse<PagedHearingResult<HearingDetailDTO>> { Success = true, Data = result });
        }

        // =====================================================
        // GET CASE HEARINGS — Any firm user
        // Returns a paged list of every hearing (past and future) recorded
        // against a specific case.
        // =====================================================
        [HttpGet("case/{caseId}")]
        [Authorize(Roles = RoleNames.AllFirmUsers)]
        [ProducesResponseType(typeof(ApiResponse<PagedHearingResult<HearingDetailDTO>>), 200)]
        public async Task<IActionResult> GetCaseHearings(
            long caseId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var query = new GetCaseHearingsQuery
            {
                CaseId = caseId,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
            var result = await _mediator.Send(query);
            return Ok(new ApiResponse<PagedHearingResult<HearingDetailDTO>> { Success = true, Data = result });
        }

        // =====================================================
        // UPDATE HEARING — Lawyer roles
        // Edits an existing hearing's date, court, purpose, outcome, or
        // next-hearing date.
        // =====================================================
        [HttpPut("{id}")]
        [Authorize(Roles = RoleNames.AllLawyers)]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 400)]
        public async Task<IActionResult> UpdateHearing(long id, [FromBody] UpdateHearingDTO dto)
        {
            dto.HearingId = id;
            var command = new UpdateHearingCommand { Hearing = dto };
            var result = await _mediator.Send(command);
            return Ok(new ApiResponse<bool> { Success = result, Data = result, Message = "Hearing updated successfully" });
        }

        // =====================================================
        // DELETE HEARING — Partner and above
        // Permanently removes a hearing record. Restricted to
        // Partner-and-above since deleting hearing history is higher-risk
        // than creating/updating one.
        // =====================================================
        [HttpDelete("{id}")]
        [Authorize(Roles = RoleNames.PartnerAndAbove)]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 404)]
        public async Task<IActionResult> DeleteHearing(long id)
        {
            var command = new DeleteHearingCommand { HearingId = id };
            var result = await _mediator.Send(command);
            return Ok(new ApiResponse<bool> { Success = result, Data = result, Message = "Hearing deleted successfully" });
        }

        // HEARING ATTENDANCE
        // SRS Reference: Complete Database Schema - HearingAttendance table
        // Fixes Critical Issue: "Hearing Attendance Tracking" had model but no API

        // =====================================================
        // RECORD ATTENDANCE — Any firm user
        // Logs who was present/absent at a given hearing (attendance role,
        // present flag, arrival/departure time, remarks).
        // =====================================================
        [HttpPost("{hearingId}/attendance")]
        [Authorize(Roles = RoleNames.AllFirmUsers)]
        [ProducesResponseType(typeof(ApiResponse<long>), 201)]
        public async Task<IActionResult> RecordAttendance(long hearingId, [FromBody] RecordAttendanceDTO dto)
        {
            dto.HearingId = hearingId;
            var attendanceId = await _mediator.Send(new RecordAttendanceCommand { Attendance = dto });
            return CreatedAtAction(nameof(GetHearingAttendance), new { hearingId },
                new ApiResponse<long> { Success = true, Data = attendanceId, Message = "Attendance recorded successfully" });
        }

        // =====================================================
        // GET HEARING ATTENDANCE — Any firm user
        // Lists every attendance record logged for a given hearing.
        // =====================================================
        [HttpGet("{hearingId}/attendance")]
        [Authorize(Roles = RoleNames.AllFirmUsers)]
        [ProducesResponseType(typeof(ApiResponse<List<HearingAttendanceDTO>>), 200)]
        public async Task<IActionResult> GetHearingAttendance(long hearingId)
        {
            var result = await _mediator.Send(new GetHearingAttendanceQuery { HearingId = hearingId });
            return Ok(new ApiResponse<List<HearingAttendanceDTO>> { Success = true, Data = result });
        }

        // =====================================================
        // UPDATE ATTENDANCE — Any firm user
        // Edits an existing attendance record (e.g. correcting present
        // flag or arrival time).
        // =====================================================
        [HttpPut("{hearingId}/attendance/{attendanceId}")]
        [Authorize(Roles = RoleNames.AllFirmUsers)]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        public async Task<IActionResult> UpdateAttendance(long hearingId, long attendanceId, [FromBody] UpdateAttendanceCommand command)
        {
            command.AttendanceId = attendanceId;
            var result = await _mediator.Send(command);
            return Ok(new ApiResponse<bool> { Success = result, Data = result, Message = "Attendance updated successfully" });
        }

        // =====================================================
        // DELETE ATTENDANCE — Partner and above
        // Permanently removes an attendance record.
        // =====================================================
        [HttpDelete("{hearingId}/attendance/{attendanceId}")]
        [Authorize(Roles = RoleNames.PartnerAndAbove)]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        public async Task<IActionResult> DeleteAttendance(long hearingId, long attendanceId)
        {
            var result = await _mediator.Send(new DeleteAttendanceCommand { AttendanceId = attendanceId });
            return Ok(new ApiResponse<bool> { Success = result, Data = result, Message = "Attendance deleted successfully" });
        }
    }
}
