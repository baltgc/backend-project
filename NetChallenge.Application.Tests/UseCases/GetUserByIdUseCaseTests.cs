using Moq;
using NetChallenge.Application.DTOs;
using NetChallenge.Application.Interfaces;
using NetChallenge.Application.UseCases;

namespace NetChallenge.Application.Tests.UseCases;

public class GetUserByIdUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldReturnUser_WhenUserExists()
    {
        // Arrange
        var mockUserService = new Mock<IUserService>();
        var userId = 1;
        var expectedUser = new UserDto
        {
            Id = userId,
            Name = "John Doe",
            Username = "johndoe",
            Email = "john@example.com",
            Phone = "123-456-7890",
            Website = "johndoe.com"
        };

        mockUserService.Setup(s => s.GetUserByIdAsync(userId))
            .ReturnsAsync(expectedUser);

        var useCase = new GetUserByIdUseCase(mockUserService.Object);

        // Act
        var result = await useCase.ExecuteAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.Id);
        Assert.Equal("John Doe", result.Name);
        mockUserService.Verify(s => s.GetUserByIdAsync(userId), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnNull_WhenUserDoesNotExist()
    {
        // Arrange
        var mockUserService = new Mock<IUserService>();
        var userId = 999;

        mockUserService.Setup(s => s.GetUserByIdAsync(userId))
            .ReturnsAsync((UserDto?)null);

        var useCase = new GetUserByIdUseCase(mockUserService.Object);

        // Act
        var result = await useCase.ExecuteAsync(userId);

        // Assert
        Assert.Null(result);
        mockUserService.Verify(s => s.GetUserByIdAsync(userId), Times.Once);
    }
}

