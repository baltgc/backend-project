using Moq;
using NetChallenge.Application.DTOs;
using NetChallenge.Application.Interfaces;
using NetChallenge.Application.UseCases;

namespace NetChallenge.Application.Tests.UseCases;

public class GetUsersUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldReturnUsers_WhenServiceReturnsUsers()
    {
        // Arrange
        var mockUserService = new Mock<IUserService>();
        var expectedUsers = new List<UserDto>
        {
            new UserDto { Id = 1, Name = "John Doe", Username = "johndoe", Email = "john@example.com" },
            new UserDto { Id = 2, Name = "Jane Smith", Username = "janesmith", Email = "jane@example.com" }
        };

        mockUserService.Setup(s => s.GetUsersAsync())
            .ReturnsAsync(expectedUsers);

        var useCase = new GetUsersUseCase(mockUserService.Object);

        // Act
        var result = await useCase.ExecuteAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.Equal("John Doe", result.First().Name);
        mockUserService.Verify(s => s.GetUsersAsync(), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnEmptyList_WhenServiceReturnsEmpty()
    {
        // Arrange
        var mockUserService = new Mock<IUserService>();
        mockUserService.Setup(s => s.GetUsersAsync())
            .ReturnsAsync(Enumerable.Empty<UserDto>());

        var useCase = new GetUsersUseCase(mockUserService.Object);

        // Act
        var result = await useCase.ExecuteAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
        mockUserService.Verify(s => s.GetUsersAsync(), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldHandleLargeNumberOfUsers()
    {
        // Arrange
        var mockUserService = new Mock<IUserService>();
        var largeUserList = Enumerable.Range(1, 1000)
            .Select(i => new UserDto
            {
                Id = i,
                Name = $"User {i}",
                Username = $"user{i}",
                Email = $"user{i}@example.com"
            })
            .ToList();

        mockUserService.Setup(s => s.GetUsersAsync())
            .ReturnsAsync(largeUserList);

        var useCase = new GetUsersUseCase(mockUserService.Object);

        // Act
        var result = await useCase.ExecuteAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1000, result.Count());
        mockUserService.Verify(s => s.GetUsersAsync(), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldPropagateException_WhenServiceThrows()
    {
        // Arrange
        var mockUserService = new Mock<IUserService>();
        mockUserService.Setup(s => s.GetUsersAsync())
            .ThrowsAsync(new Exception("Service error"));

        var useCase = new GetUsersUseCase(mockUserService.Object);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => useCase.ExecuteAsync());
        mockUserService.Verify(s => s.GetUsersAsync(), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldHandleNullUsersInList()
    {
        // Arrange
        var mockUserService = new Mock<IUserService>();
        var usersWithNulls = new List<UserDto?>
        {
            new UserDto { Id = 1, Name = "User 1" },
            null,
            new UserDto { Id = 3, Name = "User 3" }
        }.Where(u => u != null).Cast<UserDto>();

        mockUserService.Setup(s => s.GetUsersAsync())
            .ReturnsAsync(usersWithNulls);

        var useCase = new GetUsersUseCase(mockUserService.Object);

        // Act
        var result = await useCase.ExecuteAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        mockUserService.Verify(s => s.GetUsersAsync(), Times.Once);
    }
}

