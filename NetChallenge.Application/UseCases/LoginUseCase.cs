using NetChallenge.Application.DTOs;
using NetChallenge.Application.Interfaces;

namespace NetChallenge.Application.UseCases;

public class LoginUseCase
{
    private readonly IAuthService _authService;

    public LoginUseCase(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<LoginResponse?> ExecuteAsync(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException("Username and password are required");
        }

        return await _authService.AuthenticateAsync(username, password);
    }
}

