using NetChallenge.Application.Interfaces;

namespace NetChallenge.Application.UseCases;

public class LogoutUseCase
{
    private readonly IAuthService _authService;

    public LogoutUseCase(IAuthService authService)
    {
        _authService = authService;
    }

    public Task ExecuteAsync(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            throw new ArgumentException("Refresh token is required.");
        }

        return _authService.RevokeAsync(refreshToken);
    }
}


