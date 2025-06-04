using EnterpriseScheduler.Controllers;
using EnterpriseScheduler.Exceptions;
using EnterpriseScheduler.Interfaces.Services;
using EnterpriseScheduler.Models;
using EnterpriseScheduler.Models.Common;
using EnterpriseScheduler.Models.DTOs.Meetings;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseScheduler.Tests.Controllers;

public class MeetingsControllerTests
{
    private readonly Mock<IMeetingService> _meetingServiceMock;
    private readonly MeetingsController _meetingsController;

    public MeetingsControllerTests()
    {
        _meetingServiceMock = new Mock<IMeetingService>();
        _meetingsController = new MeetingsController(_meetingServiceMock.Object);
    }

    [Fact]
    public async Task GetMeetingsPaginated_WithValidParameters_ReturnsOkResult()
    {
        // Arrange
        var page = 1;
        var pageSize = 10;
        var expectedResponse = new PaginatedResult<MeetingResponse>
        {
            Items = new List<MeetingResponse>(),
            TotalCount = 0
        };

        _meetingServiceMock.Setup(x => x.GetMeetingsPaginated(page, pageSize))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _meetingsController.GetMeetingsPaginated(page, pageSize);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<PaginatedResult<MeetingResponse>>(okResult.Value);
        Assert.Equal(expectedResponse, returnValue);
    }

    [Fact]
    public async Task GetUserMeetings_WithValidUserId_ReturnsOkResult()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expectedResponse = new List<MeetingResponse>();

        _meetingServiceMock.Setup(x => x.GetUserMeetings(userId))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _meetingsController.GetUserMeetings(userId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<List<MeetingResponse>>(okResult.Value);
        Assert.Equal(expectedResponse, returnValue);
    }

    [Fact]
    public async Task GetUserMeetings_WithInvalidUserId_ReturnsNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _meetingServiceMock.Setup(x => x.GetUserMeetings(userId))
            .ThrowsAsync(new KeyNotFoundException("User not found"));

        // Act
        var result = await _meetingsController.GetUserMeetings(userId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("User not found", notFoundResult.Value);
    }

    [Fact]
    public async Task GetMeeting_WithValidId_ReturnsOkResult()
    {
        // Arrange
        var meetingId = Guid.NewGuid();
        var expectedResponse = new MeetingResponse
        {
            Id = meetingId,
            Title = "Test Meeting",
            StartTime = DateTimeOffset.UtcNow,
            EndTime = DateTimeOffset.UtcNow.AddHours(1)
        };

        _meetingServiceMock.Setup(x => x.GetMeeting(meetingId))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _meetingsController.GetMeeting(meetingId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<MeetingResponse>(okResult.Value);
        Assert.Equal(expectedResponse, returnValue);
    }

    [Fact]
    public async Task GetMeeting_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var meetingId = Guid.NewGuid();
        _meetingServiceMock.Setup(x => x.GetMeeting(meetingId))
            .ThrowsAsync(new KeyNotFoundException("Meeting not found"));

        // Act
        var result = await _meetingsController.GetMeeting(meetingId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Meeting not found", notFoundResult.Value);
    }

    [Fact]
    public async Task CreateMeeting_WithInvalidModelState_ReturnsBadRequest()
    {
        // Arrange
        _meetingsController.ModelState.AddModelError("Title", "Title is required");

        var meetingRequest = new MeetingRequest
        {
            Title = "Test Meeting",
            StartTime = DateTimeOffset.UtcNow,
            EndTime = DateTimeOffset.UtcNow.AddHours(1),
            ParticipantIds = new List<Guid> { Guid.NewGuid() }
        };

        // Act
        var result = await _meetingsController.CreateMeeting(meetingRequest);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult.Value);
    }

    [Fact]
    public async Task CreateMeeting_WithValidRequest_ReturnsCreatedResult()
    {
        // Arrange
        var meetingRequest = new MeetingRequest
        {
            Title = "Test Meeting",
            StartTime = DateTimeOffset.UtcNow,
            EndTime = DateTimeOffset.UtcNow.AddHours(1),
            ParticipantIds = new List<Guid> { Guid.NewGuid() }
        };
        var expectedResponse = new MeetingResponse
        {
            Id = Guid.NewGuid(),
            Title = "Test Meeting",
            StartTime = DateTimeOffset.UtcNow,
            EndTime = DateTimeOffset.UtcNow.AddHours(1)
        };

        _meetingServiceMock.Setup(x => x.CreateMeeting(meetingRequest))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _meetingsController.CreateMeeting(meetingRequest);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        var returnValue = Assert.IsType<MeetingResponse>(createdResult.Value);
        Assert.Equal(expectedResponse, returnValue);
        Assert.Equal(nameof(MeetingsController.GetMeeting), createdResult.ActionName);
        Assert.NotNull(createdResult.RouteValues);
        Assert.Equal(expectedResponse.Id, createdResult.RouteValues["id"]);
    }

    [Fact]
    public async Task CreateMeeting_WithTimeConflict_ReturnsConflict()
    {
        // Arrange
        var meetingRequest = new MeetingRequest
        {
            Title = "Test Meeting",
            StartTime = DateTimeOffset.UtcNow,
            EndTime = DateTimeOffset.UtcNow.AddHours(1),
            ParticipantIds = new List<Guid> { Guid.NewGuid() }
        };
        var availableSlots = new List<TimeSlot>
        {
            new TimeSlot { StartTime = DateTimeOffset.UtcNow, EndTime = DateTimeOffset.UtcNow.AddHours(1) }
        };

        _meetingServiceMock.Setup(x => x.CreateMeeting(meetingRequest))
            .ThrowsAsync(new MeetingConflictException(availableSlots));

        // Act
        var result = await _meetingsController.CreateMeeting(meetingRequest);

        // Assert
        Assert.IsType<ConflictObjectResult>(result);
    }

    [Fact]
    public async Task CreateMeeting_WithInvalidRequest_ReturnsBadRequest()
    {
        // Arrange
        var meetingRequest = new MeetingRequest
        {
            Title = "Test Meeting",
            StartTime = DateTimeOffset.UtcNow,
            EndTime = DateTimeOffset.UtcNow.AddHours(1),
            ParticipantIds = new List<Guid> { Guid.NewGuid() }
        };

        _meetingServiceMock.Setup(x => x.CreateMeeting(meetingRequest))
            .ThrowsAsync(new ArgumentException("Invalid meeting request"));

        // Act
        var result = await _meetingsController.CreateMeeting(meetingRequest);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Invalid meeting request", badRequestResult.Value);
    }

    [Fact]
    public async Task UpdateMeeting_WithInvalidModelState_ReturnsBadRequest()
    {
        // Arrange
        _meetingsController.ModelState.AddModelError("Title", "Title is required");

        var meetingId = Guid.NewGuid();
        var meetingRequest = new MeetingRequest
        {
            Title = "Updated Meeting",
            StartTime = DateTimeOffset.UtcNow,
            EndTime = DateTimeOffset.UtcNow.AddHours(1),
            ParticipantIds = new List<Guid> { Guid.NewGuid() }
        };

        // Act
        var result = await _meetingsController.UpdateMeeting(meetingId, meetingRequest);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult.Value);
    }

    [Fact]
    public async Task UpdateMeeting_WithValidRequest_ReturnsOkResult()
    {
        // Arrange
        var meetingId = Guid.NewGuid();
        var meetingRequest = new MeetingRequest
        {
            Title = "Updated Meeting",
            StartTime = DateTimeOffset.UtcNow,
            EndTime = DateTimeOffset.UtcNow.AddHours(1),
            ParticipantIds = new List<Guid> { Guid.NewGuid() }
        };
        var expectedResponse = new MeetingResponse
        {
            Id = meetingId,
            Title = "Updated Meeting",
            StartTime = DateTimeOffset.UtcNow,
            EndTime = DateTimeOffset.UtcNow.AddHours(1)
        };

        _meetingServiceMock.Setup(x => x.UpdateMeeting(meetingId, meetingRequest))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _meetingsController.UpdateMeeting(meetingId, meetingRequest);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<MeetingResponse>(okResult.Value);
        Assert.Equal(expectedResponse, returnValue);
    }

    [Fact]
    public async Task UpdateMeeting_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var meetingId = Guid.NewGuid();
        var meetingRequest = new MeetingRequest
        {
            Title = "Updated Meeting",
            StartTime = DateTimeOffset.UtcNow,
            EndTime = DateTimeOffset.UtcNow.AddHours(1),
            ParticipantIds = new List<Guid> { Guid.NewGuid() }
        };

        _meetingServiceMock.Setup(x => x.UpdateMeeting(meetingId, meetingRequest))
            .ThrowsAsync(new KeyNotFoundException("Meeting not found"));

        // Act
        var result = await _meetingsController.UpdateMeeting(meetingId, meetingRequest);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Meeting not found", notFoundResult.Value);
    }

    [Fact]
    public async Task UpdateMeeting_WithInvalidRequest_ReturnsBadRequest()
    {
        // Arrange
        var meetingId = Guid.NewGuid();
        var meetingRequest = new MeetingRequest
        {
            Title = "Updated Meeting",
            StartTime = DateTimeOffset.UtcNow,
            EndTime = DateTimeOffset.UtcNow.AddHours(1),
            ParticipantIds = new List<Guid> { Guid.NewGuid() }
        };

        _meetingServiceMock.Setup(x => x.UpdateMeeting(meetingId, meetingRequest))
            .ThrowsAsync(new ArgumentException("Invalid meeting request"));

        // Act
        var result = await _meetingsController.UpdateMeeting(meetingId, meetingRequest);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Invalid meeting request", badRequestResult.Value);
    }

    [Fact]
    public async Task DeleteMeeting_WithValidId_ReturnsNoContent()
    {
        // Arrange
        var meetingId = Guid.NewGuid();
        _meetingServiceMock.Setup(x => x.DeleteMeeting(meetingId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _meetingsController.DeleteMeeting(meetingId);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task DeleteMeeting_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var meetingId = Guid.NewGuid();
        _meetingServiceMock.Setup(x => x.DeleteMeeting(meetingId))
            .ThrowsAsync(new KeyNotFoundException("Meeting not found"));

        // Act
        var result = await _meetingsController.DeleteMeeting(meetingId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Meeting not found", notFoundResult.Value);
    }
}
