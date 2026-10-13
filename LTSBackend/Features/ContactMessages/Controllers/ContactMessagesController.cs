using LTSBackend.Comman.Responses;
using LTSBackend.Features.ContactMessages.Commands.SubmitContactMessage;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace LTSBackend.Features.ContactMessages.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ContactMessagesController(IMediator _mediator, ILogger<ContactMessagesController> _logger) : ControllerBase
{








    [HttpPost]
    [AllowAnonymous]
    [EnableRateLimiting("auth-moderate")]
    public async Task<IActionResult> Submit([FromBody] SubmitContactMessageCommand command)
    {
        _logger.LogInformation("Contact form submitted by {Email}", command.Email);
        await _mediator.Send(command);
        return Ok(ApiResponse<bool>.SuccessResponse(true, "Your message has been sent. We'll get back to you soon."));
    }
}
