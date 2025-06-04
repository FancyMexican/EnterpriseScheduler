using AutoMapper;
using EnterpriseScheduler.Controllers;
using EnterpriseScheduler.Interfaces.Services;
using EnterpriseScheduler.Models.Common;
using EnterpriseScheduler.Models.DTOs.Users;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseScheduler.Tests.Controllers;

public class UsersControllerTests
{
    private readonly Mock<IUserService> _userServiceMock;
    private readonly UsersController _usersController;

    public UsersControllerTests()
    {
        _userServiceMock = new Mock<IUserService>();
        _usersController = new UsersController(_userServiceMock.Object);
    }

    [Fact]
    public async Task GetUsersPaginated_WithValidParameters_ReturnsOkResult()
    {
        // Arrange
        var page = 1;
        var pageSize = 10;
        var expectedResponse = new PaginatedResult<UserResponse>
        {
            Items = new List<UserResponse>(),
            TotalCount = 0
        };

        _userServiceMock.Setup(x => x.GetUsersPaginated(page, pageSize))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _usersController.GetUsersPaginated(page, pageSize);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<PaginatedResult<UserResponse>>(okResult.Value);
        Assert.Equal(expectedResponse, returnValue);
    }

    [Fact]
    public async Task GetUser_WithValidId_ReturnsOkResult()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expectedResponse = new UserResponse
        {
            Id = userId,
            Name = "Test User",
            TimeZone = "UTC"
        };

        _userServiceMock.Setup(x => x.GetUser(userId))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _usersController.GetUser(userId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<UserResponse>(okResult.Value);
        Assert.Equal(expectedResponse, returnValue);
    }

    [Fact]
    public async Task GetUser_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userServiceMock.Setup(x => x.GetUser(userId))
            .ThrowsAsync(new KeyNotFoundException("User not found"));

        // Act
        var result = await _usersController.GetUser(userId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("User not found", notFoundResult.Value);
    }

    [Fact]
    public async Task CreateUser_WithInvalidModel_ReturnsBadRequest()
    {
        // Arrange
        var userRequest = new UserRequest
        {
            Name = "", // Invalid name
            TimeZone = "UTC"
        };
        _usersController.ModelState.AddModelError("Name", "Name is required");

        // Act
        var result = await _usersController.CreateUser(userRequest);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult.Value);
    }

    [Fact]
    public async Task CreateUser_WithValidRequest_ReturnsCreatedResult()
    {
        // Arrange
        var userRequest = new UserRequest
        {
            Name = "Test User",
            TimeZone = "UTC"
        };
        var expectedResponse = new UserResponse
        {
            Id = Guid.NewGuid(),
            Name = "Test User",
            TimeZone = "UTC"
        };

        _userServiceMock.Setup(x => x.CreateUser(userRequest))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _usersController.CreateUser(userRequest);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        var returnValue = Assert.IsType<UserResponse>(createdResult.Value);
        Assert.Equal(expectedResponse, returnValue);
        Assert.Equal(nameof(UsersController.GetUser), createdResult.ActionName);
        Assert.NotNull(createdResult.RouteValues);
        Assert.Equal(expectedResponse.Id, createdResult.RouteValues["id"]);
    }

    [Fact]
    public async Task UpdateUser_WithInvalidModel_ReturnsBadRequest()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userRequest = new UserRequest
        {
            Name = "", // Invalid name
            TimeZone = "UTC"
        };
        _usersController.ModelState.AddModelError("Name", "Name is required");

        // Act
        var result = await _usersController.UpdateUser(userId, userRequest);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult.Value);
    }

    [Fact]
    public async Task UpdateUser_WithValidRequest_ReturnsOkResult()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userRequest = new UserRequest
        {
            Name = "Updated User",
            TimeZone = "UTC"
        };
        var expectedResponse = new UserResponse
        {
            Id = userId,
            Name = "Updated User",
            TimeZone = "UTC"
        };

        _userServiceMock.Setup(x => x.UpdateUser(userId, userRequest))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _usersController.UpdateUser(userId, userRequest);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<UserResponse>(okResult.Value);
        Assert.Equal(expectedResponse, returnValue);
    }

    [Fact]
    public async Task UpdateUser_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userRequest = new UserRequest
        {
            Name = "Updated User",
            TimeZone = "UTC"
        };

        _userServiceMock.Setup(x => x.UpdateUser(userId, userRequest))
            .ThrowsAsync(new KeyNotFoundException("User not found"));

        // Act
        var result = await _usersController.UpdateUser(userId, userRequest);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("User not found", notFoundResult.Value);
    }

    [Fact]
    public async Task DeleteUser_WithValidId_ReturnsNoContent()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userServiceMock.Setup(x => x.DeleteUser(userId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _usersController.DeleteUser(userId);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task DeleteUser_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userServiceMock.Setup(x => x.DeleteUser(userId))
            .ThrowsAsync(new KeyNotFoundException("User not found"));

        // Act
        var result = await _usersController.DeleteUser(userId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("User not found", notFoundResult.Value);
    }
}
