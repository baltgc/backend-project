using NetChallenge.Application.DTOs;
using NetChallenge.Application.Interfaces;

namespace NetChallenge.Application.UseCases;

public class RefreshTokenUseCase
{
    private readonly IAuthService _authService;

    public RefreshTokenUseCase(IAuthService authService)
    {
        _authService = authService;
    }

    public Task<LoginResponse?> ExecuteAsync(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            throw new ArgumentException("Refresh token is required.");
        }

        return _authService.RefreshAsync(refreshToken);
    }
}


