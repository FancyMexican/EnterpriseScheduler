using AutoMapper;
using EnterpriseScheduler.Interfaces.Repositories;
using EnterpriseScheduler.Interfaces.Services;
using EnterpriseScheduler.Models;
using EnterpriseScheduler.Models.Common;
using EnterpriseScheduler.Models.DTOs.Users;
using EnterpriseScheduler.Services;

namespace EnterpriseScheduler.Tests.Services;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly IUserService _userService;

    public UserServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _mapperMock = new Mock<IMapper>();
        _userService = new UserService(_userRepositoryMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task GetUsersPaginated_WithValidParameters_ReturnsPaginatedResult()
    {
        // Arrange
        var page = 1;
        var pageSize = 10;
        var users = new List<User>();
        var paginatedUsers = new PaginatedResult<User>
        {
            Items = users,
            TotalCount = 0,
            PageNumber = page,
            PageSize = pageSize
        };
        var expectedResponse = new PaginatedResult<UserResponse>
        {
            Items = new List<UserResponse>(),
            TotalCount = 0,
            PageNumber = page,
            PageSize = pageSize
        };

        _userRepositoryMock.Setup(x => x.GetPaginatedAsync(page, pageSize))
            .ReturnsAsync(paginatedUsers);
        _mapperMock.Setup(x => x.Map<PaginatedResult<UserResponse>>(paginatedUsers))
            .Returns(expectedResponse);

        // Act
        var result = await _userService.GetUsersPaginated(page, pageSize);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedResponse, result);
    }

    [Fact]
    public async Task GetUser_WithValidId_ReturnsUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Name = "Test User",
            TimeZone = "UTC"
        };
        var expectedResponse = new UserResponse
        {
            Id = userId,
            Name = "Test User",
            TimeZone = "UTC"
        };

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);
        _mapperMock.Setup(x => x.Map<UserResponse>(user))
            .Returns(expectedResponse);

        // Act
        var result = await _userService.GetUser(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedResponse, result);
    }

    [Fact]
    public async Task CreateUser_WithValidRequest_ReturnsCreatedUser()
    {
        // Arrange
        var userRequest = new UserRequest
        {
            Name = "Test User",
            TimeZone = "UTC"
        };
        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = "Test User",
            TimeZone = "UTC"
        };
        var expectedResponse = new UserResponse
        {
            Id = user.Id,
            Name = "Test User",
            TimeZone = "UTC"
        };

        _mapperMock.Setup(x => x.Map<User>(userRequest))
            .Returns(user);
        _mapperMock.Setup(x => x.Map<UserResponse>(user))
            .Returns(expectedResponse);

        // Act
        var result = await _userService.CreateUser(userRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedResponse, result);
        _userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task UpdateUser_WithValidRequest_ReturnsUpdatedUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userRequest = new UserRequest
        {
            Name = "Updated User",
            TimeZone = "UTC"
        };
        var existingUser = new User
        {
            Id = userId,
            Name = "Test User",
            TimeZone = "UTC"
        };
        var expectedResponse = new UserResponse
        {
            Id = userId,
            Name = "Updated User",
            TimeZone = "UTC"
        };

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(existingUser);
        _mapperMock.Setup(x => x.Map(userRequest, existingUser))
            .Callback<UserRequest, User>((req, user) =>
            {
                user.Name = req.Name;
                user.TimeZone = req.TimeZone;
            });
        _mapperMock.Setup(x => x.Map<UserResponse>(It.Is<User>(u => u.Id == userId && u.Name == "Updated User")))
            .Returns(expectedResponse);

        // Act
        var result = await _userService.UpdateUser(userId, userRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedResponse, result);
        _userRepositoryMock.Verify(x => x.UpdateAsync(It.Is<User>(u => u.Id == userId && u.Name == "Updated User")), Times.Once);
        _mapperMock.Verify(x => x.Map<UserResponse>(It.Is<User>(u => u.Id == userId && u.Name == "Updated User")), Times.Once);
    }

    [Fact]
    public async Task DeleteUser_WithValidId_DeletesUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Name = "Test User",
            TimeZone = "UTC"
        };

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);

        // Act
        await _userService.DeleteUser(userId);

        // Assert
        _userRepositoryMock.Verify(x => x.DeleteAsync(user), Times.Once);
    }

    [Theory]
    [InlineData(0, 10)] // Invalid page
    [InlineData(1, 0)] // Invalid page size
    [InlineData(1, 101)] // Page size too large
    public async Task GetUsersPaginated_WithInvalidParameters_AdjustsToValidRange(int page, int pageSize)
    {
        // Arrange
        var users = new List<User>();
        var paginatedUsers = new PaginatedResult<User>
        {
            Items = users,
            TotalCount = 0,
            PageNumber = 1,
            PageSize = 10
        };
        var expectedResponse = new PaginatedResult<UserResponse>
        {
            Items = new List<UserResponse>(),
            TotalCount = 0,
            PageNumber = 1,
            PageSize = 10
        };

        _userRepositoryMock.Setup(x => x.GetPaginatedAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(paginatedUsers);
        _mapperMock.Setup(x => x.Map<PaginatedResult<UserResponse>>(paginatedUsers))
            .Returns(expectedResponse);

        // Act
        var result = await _userService.GetUsersPaginated(page, pageSize);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedResponse, result);
    }
}
