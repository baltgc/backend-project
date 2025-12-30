using Moq;
using NetChallenge.Application.DTOs;
using NetChallenge.Application.Interfaces;
using NetChallenge.Application.UseCases;

namespace NetChallenge.Application.Tests.UseCases;

public class LoginUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldReturnLoginResponse_WhenCredentialsAreValid()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var username = "testuser";
        var password = "testpass";
        var expectedResponse = new LoginResponse
        {
            Token = "test-token-12345",
            ExpiresAt = DateTime.UtcNow.AddMinutes(60),
        };

        mockAuthService
            .Setup(s => s.AuthenticateAsync(username, password))
            .ReturnsAsync(expectedResponse);

        var useCase = new LoginUseCase(mockAuthService.Object);

        // Act
        var result = await useCase.ExecuteAsync(username, password);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("test-token-12345", result.Token);
        Assert.True(result.ExpiresAt > DateTime.UtcNow);
        mockAuthService.Verify(s => s.AuthenticateAsync(username, password), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnNull_WhenCredentialsAreInvalid()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var username = "invaliduser";
        var password = "invalidpass";

        mockAuthService
            .Setup(s => s.AuthenticateAsync(username, password))
            .ReturnsAsync((LoginResponse?)null);

        var useCase = new LoginUseCase(mockAuthService.Object);

        // Act
        var result = await useCase.ExecuteAsync(username, password);

        // Assert
        Assert.Null(result);
        mockAuthService.Verify(s => s.AuthenticateAsync(username, password), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowArgumentException_WhenUsernameIsEmpty()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var useCase = new LoginUseCase(mockAuthService.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => useCase.ExecuteAsync("", "password"));
        mockAuthService.Verify(
            s => s.AuthenticateAsync(It.IsAny<string>(), It.IsAny<string>()),
            Times.Never
        );
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowArgumentException_WhenPasswordIsEmpty()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var useCase = new LoginUseCase(mockAuthService.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => useCase.ExecuteAsync("username", ""));
        mockAuthService.Verify(
            s => s.AuthenticateAsync(It.IsAny<string>(), It.IsAny<string>()),
            Times.Never
        );
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowArgumentException_WhenUsernameIsWhitespace()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var useCase = new LoginUseCase(mockAuthService.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => useCase.ExecuteAsync("   ", "password"));
        mockAuthService.Verify(
            s => s.AuthenticateAsync(It.IsAny<string>(), It.IsAny<string>()),
            Times.Never
        );
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowArgumentException_WhenPasswordIsWhitespace()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var useCase = new LoginUseCase(mockAuthService.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => useCase.ExecuteAsync("username", "   "));
        mockAuthService.Verify(
            s => s.AuthenticateAsync(It.IsAny<string>(), It.IsAny<string>()),
            Times.Never
        );
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowArgumentException_WhenUsernameIsNull()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var useCase = new LoginUseCase(mockAuthService.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => useCase.ExecuteAsync(null!, "password"));
        mockAuthService.Verify(
            s => s.AuthenticateAsync(It.IsAny<string>(), It.IsAny<string>()),
            Times.Never
        );
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowArgumentException_WhenPasswordIsNull()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var useCase = new LoginUseCase(mockAuthService.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => useCase.ExecuteAsync("username", null!));
        mockAuthService.Verify(
            s => s.AuthenticateAsync(It.IsAny<string>(), It.IsAny<string>()),
            Times.Never
        );
    }

    [Fact]
    public async Task ExecuteAsync_ShouldHandleVeryLongUsername()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var longUsername = new string('a', 1000);
        var expectedResponse = new LoginResponse
        {
            Token = "test-token",
            ExpiresAt = DateTime.UtcNow.AddMinutes(60),
        };

        mockAuthService
            .Setup(s => s.AuthenticateAsync(longUsername, "password"))
            .ReturnsAsync(expectedResponse);

        var useCase = new LoginUseCase(mockAuthService.Object);

        // Act
        var result = await useCase.ExecuteAsync(longUsername, "password");

        // Assert
        Assert.NotNull(result);
        mockAuthService.Verify(s => s.AuthenticateAsync(longUsername, "password"), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldHandleVeryLongPassword()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var longPassword = new string('b', 1000);
        var expectedResponse = new LoginResponse
        {
            Token = "test-token",
            ExpiresAt = DateTime.UtcNow.AddMinutes(60),
        };

        mockAuthService
            .Setup(s => s.AuthenticateAsync("username", longPassword))
            .ReturnsAsync(expectedResponse);

        var useCase = new LoginUseCase(mockAuthService.Object);

        // Act
        var result = await useCase.ExecuteAsync("username", longPassword);

        // Assert
        Assert.NotNull(result);
        mockAuthService.Verify(s => s.AuthenticateAsync("username", longPassword), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldPropagateException_WhenServiceThrows()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        mockAuthService
            .Setup(s => s.AuthenticateAsync("username", "password"))
            .ThrowsAsync(new Exception("Service error"));

        var useCase = new LoginUseCase(mockAuthService.Object);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => useCase.ExecuteAsync("username", "password"));
        mockAuthService.Verify(s => s.AuthenticateAsync("username", "password"), Times.Once);
    }
}
