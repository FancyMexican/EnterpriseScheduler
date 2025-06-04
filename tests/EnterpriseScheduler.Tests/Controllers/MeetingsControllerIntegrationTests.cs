using System.Net;
using System.Net.Http.Json;
using EnterpriseScheduler.Data;
using EnterpriseScheduler.Models;
using EnterpriseScheduler.Models.DTOs.Meetings;
using EnterpriseScheduler.Tests.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseScheduler.Tests.Controllers;

public class MeetingsControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>, IDisposable
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly ApplicationDbContext _context;
    private readonly IServiceScope _scope;

    public MeetingsControllerIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
        _scope = _factory.Services.CreateScope();
        _context = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Ensure database is created and seeded
        _context.Database.EnsureCreated();

        // Seed test user if none exists
        if (!_context.Users.Any())
        {
            var testUser = new User
            {
                Id = Guid.NewGuid(),
                Name = "Test User",
                TimeZone = "UTC"
            };
            _context.Users.Add(testUser);
            _context.SaveChanges();
        }
    }

    [Fact]
    public async Task CreateMeeting_WithValidRequest_ReturnsCreatedResult()
    {
        // Arrange
        var testUser = await _context.Users.FirstOrDefaultAsync();
        Assert.NotNull(testUser);

        var meetingRequest = new MeetingRequest
        {
            Title = "Test Integration Meeting",
            StartTime = DateTimeOffset.UtcNow.AddHours(1),
            EndTime = DateTimeOffset.UtcNow.AddHours(2),
            ParticipantIds = new List<Guid> { testUser.Id }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/v1/EnterpriseScheduler/Meetings", meetingRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var createdMeeting = await response.Content.ReadFromJsonAsync<MeetingResponse>();
        Assert.NotNull(createdMeeting);
        Assert.Equal(meetingRequest.Title, createdMeeting.Title);
        Assert.Equal(meetingRequest.StartTime, createdMeeting.StartTime);
        Assert.Equal(meetingRequest.EndTime, createdMeeting.EndTime);
    }

    [Fact]
    public async Task CreateMeeting_WithTimeConflict_ReturnsConflict()
    {
        // Arrange
        var testUser = await _context.Users.FirstOrDefaultAsync();
        Assert.NotNull(testUser);

        var startTime = DateTimeOffset.UtcNow.AddHours(1);
        var endTime = DateTimeOffset.UtcNow.AddHours(2);

        // Create first meeting
        var firstMeetingRequest = new MeetingRequest
        {
            Title = "First Meeting",
            StartTime = startTime,
            EndTime = endTime,
            ParticipantIds = new List<Guid> { testUser.Id }
        };

        await _client.PostAsJsonAsync("/v1/EnterpriseScheduler/Meetings", firstMeetingRequest);

        // Create conflicting meeting
        var conflictingMeetingRequest = new MeetingRequest
        {
            Title = "Conflicting Meeting",
            StartTime = startTime.AddMinutes(30), // Overlaps with first meeting
            EndTime = endTime.AddMinutes(30),
            ParticipantIds = new List<Guid> { testUser.Id }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/v1/EnterpriseScheduler/Meetings", conflictingMeetingRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var errorMessage = await response.Content.ReadAsStringAsync();
        Assert.Contains("Conflicts with existing meetings", errorMessage);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        _scope.Dispose();
    }
}
