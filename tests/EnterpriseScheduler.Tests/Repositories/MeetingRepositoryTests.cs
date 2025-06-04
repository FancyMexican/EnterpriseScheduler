using EnterpriseScheduler.Data;
using EnterpriseScheduler.Models;
using EnterpriseScheduler.Models.Common;
using EnterpriseScheduler.Repositories;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace EnterpriseScheduler.Tests.Repositories;

public class MeetingRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly MeetingRepository _meetingRepository;

    public MeetingRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _meetingRepository = new MeetingRepository(_context);
    }

    [Fact]
    public async Task GetPaginatedAsync_WithValidParameters_ReturnsPaginatedResult()
    {
        // Arrange
        var meetings = new List<Meeting>
        {
            new Meeting
            {
                Id = Guid.NewGuid(),
                Title = "Meeting 1",
                StartTime = DateTimeOffset.UtcNow,
                EndTime = DateTimeOffset.UtcNow.AddHours(1),
                Participants = new List<User>()
            },
            new Meeting
            {
                Id = Guid.NewGuid(),
                Title = "Meeting 2",
                StartTime = DateTimeOffset.UtcNow.AddHours(2),
                EndTime = DateTimeOffset.UtcNow.AddHours(3),
                Participants = new List<User>()
            },
            new Meeting
            {
                Id = Guid.NewGuid(),
                Title = "Meeting 3",
                StartTime = DateTimeOffset.UtcNow.AddHours(4),
                EndTime = DateTimeOffset.UtcNow.AddHours(5),
                Participants = new List<User>()
            }
        };
        await _context.Meetings.AddRangeAsync(meetings);
        await _context.SaveChangesAsync();

        // Act
        var result = await _meetingRepository.GetPaginatedAsync(1, 2);

        // Assert
        Assert.Equal(2, result.Items.Count());
        Assert.Equal(3, result.TotalCount);
        Assert.Equal(1, result.PageNumber);
        Assert.Equal(2, result.PageSize);
    }

    [Fact]
    public async Task GetUserMeetings_WithValidUserId_ReturnsUserMeetings()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, Name = "Test User", TimeZone = "UTC" };
        var meetings = new List<Meeting>
        {
            new Meeting
            {
                Id = Guid.NewGuid(),
                Title = "User Meeting 1",
                StartTime = DateTimeOffset.UtcNow,
                EndTime = DateTimeOffset.UtcNow.AddHours(1),
                Participants = new List<User> { user }
            },
            new Meeting
            {
                Id = Guid.NewGuid(),
                Title = "User Meeting 2",
                StartTime = DateTimeOffset.UtcNow.AddHours(2),
                EndTime = DateTimeOffset.UtcNow.AddHours(3),
                Participants = new List<User> { user }
            }
        };
        await _context.Users.AddAsync(user);
        await _context.Meetings.AddRangeAsync(meetings);
        await _context.SaveChangesAsync();

        // Act
        var result = await _meetingRepository.GetUserMeetings(userId);

        // Assert
        Assert.Equal(2, result.Count());
        Assert.All(result, m => Assert.Contains(m.Participants, p => p.Id == userId));
    }

    [Fact]
    public async Task GetMeetingsInTimeRange_WithValidParameters_ReturnsMeetings()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, Name = "Test User", TimeZone = "UTC" };
        var startTime = DateTimeOffset.UtcNow;
        var endTime = startTime.AddHours(4);
        var meetings = new List<Meeting>
        {
            new Meeting
            {
                Id = Guid.NewGuid(),
                Title = "Meeting 1",
                StartTime = startTime.AddHours(1),
                EndTime = startTime.AddHours(2),
                Participants = new List<User> { user }
            },
            new Meeting
            {
                Id = Guid.NewGuid(),
                Title = "Meeting 2",
                StartTime = startTime.AddHours(2),
                EndTime = startTime.AddHours(3),
                Participants = new List<User> { user }
            },
            new Meeting
            {
                Id = Guid.NewGuid(),
                Title = "Meeting 3",
                StartTime = startTime.AddHours(5),
                EndTime = startTime.AddHours(6),
                Participants = new List<User> { user }
            }
        };
        await _context.Users.AddAsync(user);
        await _context.Meetings.AddRangeAsync(meetings);
        await _context.SaveChangesAsync();

        // Act
        var result = await _meetingRepository.GetMeetingsInTimeRange(startTime, endTime, new[] { userId });

        // Assert
        Assert.Equal(2, result.Count());
        Assert.All(result, m => Assert.True(m.StartTime < endTime && m.EndTime > startTime));
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsMeeting()
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
        await _context.Meetings.AddAsync(meeting);
        await _context.SaveChangesAsync();

        // Act
        var result = await _meetingRepository.GetByIdAsync(meetingId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(meetingId, result.Id);
        Assert.Equal("Test Meeting", result.Title);
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ThrowsKeyNotFoundException()
    {
        // Arrange
        var meetingId = Guid.NewGuid();

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _meetingRepository.GetByIdAsync(meetingId));
    }

    [Fact]
    public async Task AddAsync_WithValidMeeting_AddsMeeting()
    {
        // Arrange
        var meeting = new Meeting
        {
            Id = Guid.NewGuid(),
            Title = "Test Meeting",
            StartTime = DateTimeOffset.UtcNow,
            EndTime = DateTimeOffset.UtcNow.AddHours(1),
            Participants = new List<User>()
        };

        // Act
        await _meetingRepository.AddAsync(meeting);

        // Assert
        var addedMeeting = await _context.Meetings.FindAsync(meeting.Id);
        Assert.NotNull(addedMeeting);
        Assert.Equal(meeting.Title, addedMeeting.Title);
    }

    [Fact]
    public async Task UpdateAsync_WithValidMeeting_UpdatesMeeting()
    {
        // Arrange
        var meeting = new Meeting
        {
            Id = Guid.NewGuid(),
            Title = "Test Meeting",
            StartTime = DateTimeOffset.UtcNow,
            EndTime = DateTimeOffset.UtcNow.AddHours(1),
            Participants = new List<User>()
        };
        await _context.Meetings.AddAsync(meeting);
        await _context.SaveChangesAsync();

        // Act
        meeting.Title = "Updated Meeting";
        await _meetingRepository.UpdateAsync(meeting);

        // Assert
        var updatedMeeting = await _context.Meetings.FindAsync(meeting.Id);
        Assert.NotNull(updatedMeeting);
        Assert.Equal("Updated Meeting", updatedMeeting.Title);
    }

    [Fact]
    public async Task DeleteAsync_WithValidMeeting_DeletesMeeting()
    {
        // Arrange
        var meeting = new Meeting
        {
            Id = Guid.NewGuid(),
            Title = "Test Meeting",
            StartTime = DateTimeOffset.UtcNow,
            EndTime = DateTimeOffset.UtcNow.AddHours(1),
            Participants = new List<User>()
        };
        await _context.Meetings.AddAsync(meeting);
        await _context.SaveChangesAsync();

        // Act
        await _meetingRepository.DeleteAsync(meeting);

        // Assert
        var deletedMeeting = await _context.Meetings.FindAsync(meeting.Id);
        Assert.Null(deletedMeeting);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
