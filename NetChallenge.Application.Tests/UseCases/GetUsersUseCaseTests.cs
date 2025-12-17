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
}

