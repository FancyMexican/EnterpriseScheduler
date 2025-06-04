using EnterpriseScheduler.Data;
using EnterpriseScheduler.Models;
using EnterpriseScheduler.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseScheduler.Tests.Repositories;

public class UserRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly UserRepository _userRepository;

    public UserRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _userRepository = new UserRepository(_context);
    }

    [Fact]
    public async Task GetPaginatedAsync_WithValidParameters_ReturnsPaginatedResult()
    {
        // Arrange
        var users = new List<User>
        {
            new User { Id = Guid.NewGuid(), Name = "User 1", TimeZone = "UTC" },
            new User { Id = Guid.NewGuid(), Name = "User 2", TimeZone = "UTC" },
            new User { Id = Guid.NewGuid(), Name = "User 3", TimeZone = "UTC" }
        };
        await _context.Users.AddRangeAsync(users);
        await _context.SaveChangesAsync();

        // Act
        var result = await _userRepository.GetPaginatedAsync(1, 2);

        // Assert
        Assert.Equal(2, result.Items.Count());
        Assert.Equal(3, result.TotalCount);
        Assert.Equal(1, result.PageNumber);
        Assert.Equal(2, result.PageSize);
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, Name = "Test User", TimeZone = "UTC" };
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _userRepository.GetByIdAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.Id);
        Assert.Equal("Test User", result.Name);
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ThrowsKeyNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _userRepository.GetByIdAsync(userId));
    }

    [Fact]
    public async Task GetByIdsAsync_WithValidIds_ReturnsUsers()
    {
        // Arrange
        var userIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var users = new List<User>
        {
            new User { Id = userIds[0], Name = "User 1", TimeZone = "UTC" },
            new User { Id = userIds[1], Name = "User 2", TimeZone = "UTC" }
        };
        await _context.Users.AddRangeAsync(users);
        await _context.SaveChangesAsync();

        // Act
        var result = await _userRepository.GetByIdsAsync(userIds);

        // Assert
        Assert.Equal(2, result.Count());
        Assert.Contains(result, u => u.Id == userIds[0]);
        Assert.Contains(result, u => u.Id == userIds[1]);
    }

    [Fact]
    public async Task AddAsync_WithValidUser_AddsUser()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid(), Name = "Test User", TimeZone = "UTC" };

        // Act
        await _userRepository.AddAsync(user);

        // Assert
        var addedUser = await _context.Users.FindAsync(user.Id);
        Assert.NotNull(addedUser);
        Assert.Equal(user.Name, addedUser.Name);
    }

    [Fact]
    public async Task UpdateAsync_WithValidUser_UpdatesUser()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid(), Name = "Test User", TimeZone = "UTC" };
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        user.Name = "Updated User";
        await _userRepository.UpdateAsync(user);

        // Assert
        var updatedUser = await _context.Users.FindAsync(user.Id);
        Assert.NotNull(updatedUser);
        Assert.Equal("Updated User", updatedUser.Name);
    }

    [Fact]
    public async Task DeleteAsync_WithValidUser_DeletesUserAndRelatedMeetings()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid(), Name = "Test User", TimeZone = "UTC" };
        var meeting = new Meeting
        {
            Id = Guid.NewGuid(),
            Title = "Test Meeting",
            StartTime = DateTimeOffset.UtcNow,
            EndTime = DateTimeOffset.UtcNow.AddHours(1),
            Participants = new List<User> { user }
        };

        await _context.Users.AddAsync(user);
        await _context.Meetings.AddAsync(meeting);
        await _context.SaveChangesAsync();

        // Act
        await _userRepository.DeleteAsync(user);

        // Assert
        var deletedUser = await _context.Users.FindAsync(user.Id);
        var deletedMeeting = await _context.Meetings.FindAsync(meeting.Id);
        Assert.Null(deletedUser);
        Assert.Null(deletedMeeting);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
