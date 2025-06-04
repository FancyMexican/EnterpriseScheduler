using EnterpriseScheduler.Constants;
using EnterpriseScheduler.Exceptions;
using EnterpriseScheduler.Interfaces.Services;
using EnterpriseScheduler.Models.DTOs.Meetings;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseScheduler.Controllers;

[ApiController]
[Route("v1/EnterpriseScheduler/[controller]")]
public class MeetingsController : ControllerBase
{
    private readonly IMeetingService _meetingService;

    public MeetingsController(IMeetingService meetingService)
    {
        _meetingService = meetingService;
    }

    [HttpGet]
    public async Task<IActionResult> GetMeetingsPaginated(
        [FromQuery] int page = PaginationConstants.DefaultPage,
        [FromQuery] int pageSize = PaginationConstants.DefaultPageSize
    )
    {
        var paginatedMeetings = await _meetingService.GetMeetingsPaginated(page, pageSize);

        return Ok(paginatedMeetings);
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetUserMeetings(Guid userId)
    {
        try
        {
            var userMeetings = await _meetingService.GetUserMeetings(userId);

            return Ok(userMeetings);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetMeeting(Guid id)
    {
        try
        {
            var meeting = await _meetingService.GetMeeting(id);

            return Ok(meeting);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateMeeting([FromBody] MeetingRequest meetingRequest)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var createdMeeting = await _meetingService.CreateMeeting(meetingRequest);
            return CreatedAtAction(nameof(GetMeeting), new { id = createdMeeting.Id }, createdMeeting);
        }
        catch (MeetingConflictException ex)
        {
            return Conflict(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateMeeting(Guid id, [FromBody] MeetingRequest meetingRequest)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var updatedMeeting = await _meetingService.UpdateMeeting(id, meetingRequest);

            return Ok(updatedMeeting);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMeeting(Guid id)
    {
        try
        {
            await _meetingService.DeleteMeeting(id);

            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }
}
