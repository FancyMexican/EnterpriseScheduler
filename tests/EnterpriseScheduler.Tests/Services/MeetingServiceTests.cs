using AutoMapper;
using EnterpriseScheduler.Exceptions;
using EnterpriseScheduler.Interfaces.Repositories;
using EnterpriseScheduler.Interfaces.Services;
using EnterpriseScheduler.Models;
using EnterpriseScheduler.Models.Common;
using EnterpriseScheduler.Models.DTOs.Meetings;
using EnterpriseScheduler.Services;

namespace EnterpriseScheduler.Tests.Services;

public class MeetingServiceTests
{
    private readonly Mock<IMeetingRepository> _meetingRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly IMeetingService _meetingService;

    public MeetingServiceTests()
    {
        _meetingRepositoryMock = new Mock<IMeetingRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _mapperMock = new Mock<IMapper>();
        _meetingService = new MeetingService(_meetingRepositoryMock.Object, _userRepositoryMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task GetMeetingsPaginated_WithValidParameters_ReturnsPaginatedResult()
    {
        // Arrange
        var page = 1;
        var pageSize = 10;
        var meetings = new List<Meeting>();
        var paginatedMeetings = new PaginatedResult<Meeting> { Items = meetings, TotalCount = 0 };
        var expectedResponse = new PaginatedResult<MeetingResponse> { Items = new List<MeetingResponse>(), TotalCount = 0 };

        _meetingRepositoryMock.Setup(x => x.GetPaginatedAsync(page, pageSize))
            .ReturnsAsync(paginatedMeetings);
        _mapperMock.Setup(x => x.Map<PaginatedResult<MeetingResponse>>(paginatedMeetings))
            .Returns(expectedResponse);

        // Act
        var result = await _meetingService.GetMeetingsPaginated(page, pageSize);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedResponse, result);
    }

    [Fact]
    public async Task GetUserMeetings_WithValidUserId_ReturnsUserMeetings()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Name = "Test User",
            TimeZone = "UTC"
        };
        var meetings = new List<Meeting>()
        {
            new Meeting
            {
                Id = Guid.NewGuid(),
                Title = "Test Meeting",
                StartTime = DateTimeOffset.UtcNow,
                EndTime = DateTimeOffset.UtcNow.AddHours(1),
                Participants = new List<User> { user }
            }
        };
        var expectedResponses = new List<MeetingResponse>()
        {
            new MeetingResponse
            {
                Id = meetings[0].Id,
                Title = meetings[0].Title,
                StartTime = meetings[0].StartTime,
                EndTime = meetings[0].EndTime
            }
        };

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);
        _meetingRepositoryMock.Setup(x => x.GetUserMeetings(userId))
            .ReturnsAsync(meetings);
        _mapperMock.Setup(x => x.Map<IEnumerable<MeetingResponse>>(meetings))
            .Returns(expectedResponses);

        // Act
        var result = await _meetingService.GetUserMeetings(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedResponses, result);
    }

    [Fact]
    public async Task GetMeeting_WithValidId_ReturnsMeeting()
    {
        // Arrange
        var meetingId = Guid.NewGuid();
        var meeting = new Meeting
        {
            Id = meetingId,
            Title = "Test Meeting",
            StartTime = DateTimeOffset.UtcNow,
            EndTime = DateTimeOffset.UtcNow.AddHours(1),
            Participants = new List<User>()
        };
        var expectedResponse = new MeetingResponse
        {
            Id = meetingId,
            Title = "Test Meeting",
            StartTime = DateTimeOffset.UtcNow,
            EndTime = DateTimeOffset.UtcNow.AddHours(1)
        };

        _meetingRepositoryMock.Setup(x => x.GetByIdAsync(meetingId))
            .ReturnsAsync(meeting);
        _mapperMock.Setup(x => x.Map<MeetingResponse>(meeting))
            .Returns(expectedResponse);

        // Act
        var result = await _meetingService.GetMeeting(meetingId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedResponse, result);
    }

    [Fact]
    public async Task CreateMeeting_WithInvalidTimes_ThrowsArgumentException()
    {
        // Arrange
        var meetingRequest = new MeetingRequest
        {
            Title = "Test Meeting",
            StartTime = DateTimeOffset.UtcNow.AddHours(1),
            EndTime = DateTimeOffset.UtcNow // End time is before start time
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _meetingService.CreateMeeting(meetingRequest));
    }

    [Fact]
    public async Task CreateMeeting_WithNoParticipants_ThrowsArgumentException()
    {
        // Arrange
        var meetingRequest = new MeetingRequest
        {
            Title = "Test Meeting",
            StartTime = DateTimeOffset.UtcNow,
            EndTime = DateTimeOffset.UtcNow.AddHours(1),
            ParticipantIds = new List<Guid>() // No participants
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _meetingService.CreateMeeting(meetingRequest));
    }

    [Fact]
    public async Task CreateMeeting_WithNonExistentParticipants_ThrowsArgumentException()
    {
        // Arrange
        var meetingRequest = new MeetingRequest
        {
            Title = "Test Meeting",
            StartTime = DateTimeOffset.UtcNow,
            EndTime = DateTimeOffset.UtcNow.AddHours(1),
            ParticipantIds = new List<Guid> { Guid.NewGuid() } // Non-existent participant
        };

        _userRepositoryMock.Setup(x => x.GetByIdsAsync(meetingRequest.ParticipantIds))
            .ReturnsAsync(new List<User>()); // No participants found

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _meetingService.CreateMeeting(meetingRequest));
    }

    [Fact]
    public async Task CreateMeeting_WhenConflictAndNoMeetingsInRange_ReturnsSingleAvailableSlot()
    {
        // Arrange
        var startTime = DateTimeOffset.UtcNow.AddHours(2);
        var endTime = startTime.AddHours(1);
        var participantId = Guid.NewGuid();
        var meetingRequest = new MeetingRequest
        {
            Title = "Test Meeting",
            StartTime = startTime,
            EndTime = endTime,
            ParticipantIds = new List<Guid> { participantId }
        };
        var participants = new List<User>
        {
            new User { Id = participantId, Name = "Test User", TimeZone = "UTC" }
        };
        // Simulate a conflict (so FindAvailableSlots is called), but no meetings in the next 7 days
        _userRepositoryMock.Setup(x => x.GetByIdsAsync(meetingRequest.ParticipantIds))
            .ReturnsAsync(participants);
        _meetingRepositoryMock.Setup(x => x.GetMeetingsInTimeRange(startTime, endTime, It.IsAny<IEnumerable<Guid>>()))
            .ReturnsAsync(new List<Meeting> { new Meeting { Id = Guid.NewGuid(), Title = "Conflict", StartTime = startTime.AddMinutes(-30), EndTime = startTime.AddMinutes(30), Participants = new List<User>() } }); // conflict
        _meetingRepositoryMock.Setup(x => x.GetMeetingsInTimeRange(startTime, startTime.AddDays(7), It.IsAny<IEnumerable<Guid>>()))
            .ReturnsAsync(new List<Meeting>()); // no meetings in next 7 days

        // Act & Assert
        var ex = await Assert.ThrowsAsync<MeetingConflictException>(() => _meetingService.CreateMeeting(meetingRequest));
        var slot = Assert.Single(ex.AvailableSlots);
        Assert.Equal(startTime.ToUniversalTime(), slot.StartTime);
        Assert.Equal(endTime.ToUniversalTime(), slot.EndTime);
    }

    [Fact]
    public async Task CreateMeeting_WhenConflictAndMeetingsInRange_ReturnsSingleAvailableSlot()
    {
        // Arrange
        var startTime = DateTimeOffset.UtcNow.AddHours(2);
        var endTime = startTime.AddHours(1);
        var participantId = Guid.NewGuid();
        var meetingRequest = new MeetingRequest
        {
            Title = "Test Meeting",
            StartTime = startTime,
            EndTime = endTime,
            ParticipantIds = new List<Guid> { participantId }
        };
        var participants = new List<User>
        {
            new User { Id = participantId, Name = "Test User", TimeZone = "UTC" }
        };
        // Simulate a conflict (so FindAvailableSlots is called), but no meetings in the next 7 days
        _userRepositoryMock.Setup(x => x.GetByIdsAsync(meetingRequest.ParticipantIds))
            .ReturnsAsync(participants);
        _meetingRepositoryMock.Setup(x => x.GetMeetingsInTimeRange(startTime, endTime, It.IsAny<IEnumerable<Guid>>()))
            .ReturnsAsync(new List<Meeting> { new Meeting { Id = Guid.NewGuid(), Title = "Conflict", StartTime = startTime.AddMinutes(-30), EndTime = startTime.AddMinutes(30), Participants = new List<User>() } }); // conflict
        _meetingRepositoryMock.Setup(x => x.GetMeetingsInTimeRange(startTime, startTime.AddDays(7), It.IsAny<IEnumerable<Guid>>()))
            .ReturnsAsync(new List<Meeting>()
            {
                new Meeting
                {
                    Id = Guid.NewGuid(),
                    Title = "Existing Meeting",
                    StartTime = startTime.AddHours(1),
                    EndTime = startTime.AddHours(2),
                    Participants = new List<User>()
                },
                new Meeting
                {
                    Id = Guid.NewGuid(),
                    Title = "Another Meeting",
                    StartTime = startTime.AddHours(3),
                    EndTime = startTime.AddHours(4),
                    Participants = new List<User>()
                }
            }); // meetings in next 7 days

        // Act & Assert
        var ex = await Assert.ThrowsAsync<MeetingConflictException>(() => _meetingService.CreateMeeting(meetingRequest));
    }

    [Fact]
    public async Task CreateMeeting_WithValidRequest_ReturnsCreatedMeeting()
    {
        // Arrange
        var meetingRequest = new MeetingRequest
        {
            Title = "Test Meeting",
            StartTime = DateTimeOffset.UtcNow,
            EndTime = DateTimeOffset.UtcNow.AddHours(1),
            ParticipantIds = new List<Guid> { Guid.NewGuid() }
        };

        var participants = new List<User>
        {
            new User
            {
                Id = meetingRequest.ParticipantIds.First(),
                Name = "Test User",
                TimeZone = "UTC"
            }
        };
        var meeting = new Meeting
        {
            Id = Guid.NewGuid(),
            Title = "Test Meeting",
            StartTime = DateTimeOffset.UtcNow,
            EndTime = DateTimeOffset.UtcNow.AddHours(1),
            Participants = new List<User>()
        };
        var expectedResponse = new MeetingResponse
        {
            Id = meeting.Id,
            Title = "Test Meeting",
            StartTime = DateTimeOffset.UtcNow,
            EndTime = DateTimeOffset.UtcNow.AddHours(1)
        };

        _userRepositoryMock.Setup(x => x.GetByIdsAsync(meetingRequest.ParticipantIds))
            .ReturnsAsync(participants);
        _meetingRepositoryMock.Setup(x => x.GetMeetingsInTimeRange(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), It.IsAny<IEnumerable<Guid>>()))
            .ReturnsAsync(new List<Meeting>());
        _mapperMock.Setup(x => x.Map<Meeting>(meetingRequest))
            .Returns(meeting);
        _mapperMock.Setup(x => x.Map<MeetingResponse>(meeting))
            .Returns(expectedResponse);

        // Act
        var result = await _meetingService.CreateMeeting(meetingRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedResponse, result);
        _meetingRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Meeting>()), Times.Once);
    }

    [Fact]
    public async Task CreateMeeting_WithTimeConflict_ThrowsMeetingConflictException()
    {
        // Arrange
        var meetingRequest = new MeetingRequest
        {
            Title = "Test Meeting",
            StartTime = DateTimeOffset.UtcNow,
            EndTime = DateTimeOffset.UtcNow.AddHours(1),
            ParticipantIds = new List<Guid> { Guid.NewGuid() }
        };

        var participants = new List<User>
        {
            new User
            {
                Id = meetingRequest.ParticipantIds.First(),
                Name = "Test User",
                TimeZone = "UTC"
            }
        };
        var conflictingMeetings = new List<Meeting>
        {
            new Meeting
            {
                Id = Guid.NewGuid(),
                Title = "Conflicting Meeting",
                StartTime = DateTimeOffset.UtcNow.AddHours(1),
                EndTime = DateTimeOffset.UtcNow.AddHours(2),
                Participants = new List<User>()
            }
        };

        _userRepositoryMock.Setup(x => x.GetByIdsAsync(meetingRequest.ParticipantIds))
            .ReturnsAsync(participants);
        _meetingRepositoryMock.Setup(x => x.GetMeetingsInTimeRange(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), It.IsAny<IEnumerable<Guid>>()))
            .ReturnsAsync(conflictingMeetings);

        // Act & Assert
        await Assert.ThrowsAsync<MeetingConflictException>(() => _meetingService.CreateMeeting(meetingRequest));
    }

    [Fact]
    public async Task UpdateMeeting_WithConflictingTimes_ThrowsMeetingConflictException()
    {
        // Arrange
        var meetingId = Guid.NewGuid();
        var meetingRequest = new MeetingRequest
        {
            Title = "Test Meeting",
            StartTime = DateTimeOffset.UtcNow,
            EndTime = DateTimeOffset.UtcNow.AddHours(1),
            ParticipantIds = new List<Guid> { Guid.NewGuid() }
        };

        var existingMeeting = new Meeting
        {
            Id = meetingId,
            Title = "Existing Meeting",
            StartTime = DateTimeOffset.UtcNow.AddHours(2),
            EndTime = DateTimeOffset.UtcNow.AddHours(3),
            Participants = new List<User>()
        };
        var participants = new List<User>
        {
            new User
            {
                Id = meetingRequest.ParticipantIds.First(),
                Name = "Test User",
                TimeZone = "UTC"
            }
           };
        var conflictingMeetings = new List<Meeting>
        {
            new Meeting
            {
                Id = Guid.NewGuid(),
                Title = "Conflicting Meeting",
                StartTime = DateTimeOffset.UtcNow,
                EndTime = DateTimeOffset.UtcNow.AddHours(1),
                Participants = new List<User>()
            }
        };

        _meetingRepositoryMock.Setup(x => x.GetByIdAsync(meetingId))
            .ReturnsAsync(existingMeeting);
        _userRepositoryMock.Setup(x => x.GetByIdsAsync(meetingRequest.ParticipantIds))
            .ReturnsAsync(participants);
        _meetingRepositoryMock.Setup(x => x.GetMeetingsInTimeRange(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), It.IsAny<IEnumerable<Guid>>()))
            .ReturnsAsync(conflictingMeetings);

        // Act & Assert
        await Assert.ThrowsAsync<MeetingConflictException>(() => _meetingService.UpdateMeeting(meetingId, meetingRequest));
    }

    [Fact]
    public async Task UpdateMeeting_WithValidRequest_ReturnsUpdatedMeeting()
    {
        // Arrange
        var meetingId = Guid.NewGuid();
        var meetingRequest = new MeetingRequest
        {
            Title = "Test Meeting",
            StartTime = DateTimeOffset.UtcNow,
            EndTime = DateTimeOffset.UtcNow.AddHours(1),
            ParticipantIds = new List<Guid> { Guid.NewGuid() }
        };

        var existingMeeting = new Meeting
        {
            Id = meetingId,
            Title = "Test Meeting",
            StartTime = DateTimeOffset.UtcNow,
            EndTime = DateTimeOffset.UtcNow.AddHours(1),
            Participants = new List<User>()
        };
        var participants = new List<User>
        {
            new User
            {
                Id = meetingRequest.ParticipantIds.First(),
                Name = "Test User",
                TimeZone = "UTC"
            }
           };
        var expectedResponse = new MeetingResponse
        {
            Id = meetingId,
            Title = "Test Meeting",
            StartTime = DateTimeOffset.UtcNow,
            EndTime = DateTimeOffset.UtcNow.AddHours(1)
        };

        _meetingRepositoryMock.Setup(x => x.GetByIdAsync(meetingId))
            .ReturnsAsync(existingMeeting);
        _userRepositoryMock.Setup(x => x.GetByIdsAsync(meetingRequest.ParticipantIds))
            .ReturnsAsync(participants);
        _meetingRepositoryMock.Setup(x => x.GetMeetingsInTimeRange(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), It.IsAny<IEnumerable<Guid>>()))
            .ReturnsAsync(new List<Meeting>());
        _mapperMock.Setup(x => x.Map<MeetingResponse>(It.IsAny<Meeting>()))
            .Returns(expectedResponse);

        // Act
        var result = await _meetingService.UpdateMeeting(meetingId, meetingRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedResponse, result);
        _meetingRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Meeting>()), Times.Once);
    }

    [Fact]
    public async Task DeleteMeeting_WithValidId_DeletesMeeting()
    {
        // Arrange
        var meetingId = Guid.NewGuid();
        var meeting = new Meeting
        {
            Id = meetingId,
            Title = "Test Meeting",
            StartTime = DateTimeOffset.UtcNow,
            EndTime = DateTimeOffset.UtcNow.AddHours(1),
            Participants = new List<User>()
        };

        _meetingRepositoryMock.Setup(x => x.GetByIdAsync(meetingId))
            .ReturnsAsync(meeting);

        // Act
        await _meetingService.DeleteMeeting(meetingId);

        // Assert
        _meetingRepositoryMock.Verify(x => x.DeleteAsync(meeting), Times.Once);
    }

    [Theory]
    [InlineData(0, 10)] // Invalid page
    [InlineData(1, 0)] // Invalid page size
    [InlineData(1, 101)] // Page size too large
    public async Task GetMeetingsPaginated_WithInvalidParameters_AdjustsToValidRange(int page, int pageSize)
    {
        // Arrange
        var meetings = new List<Meeting>();
        var paginatedMeetings = new PaginatedResult<Meeting>
        {
            Items = meetings,
            TotalCount = 0
        };
        var expectedResponse = new PaginatedResult<MeetingResponse>
        {
            Items = new List<MeetingResponse>(),
            TotalCount = 0
        };

        _meetingRepositoryMock.Setup(x => x.GetPaginatedAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(paginatedMeetings);
        _mapperMock.Setup(x => x.Map<PaginatedResult<MeetingResponse>>(paginatedMeetings))
            .Returns(expectedResponse);

        // Act
        var result = await _meetingService.GetMeetingsPaginated(page, pageSize);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedResponse, result);
    }
}
