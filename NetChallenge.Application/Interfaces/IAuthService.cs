using NetChallenge.Application.DTOs;

namespace NetChallenge.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponse?> AuthenticateAsync(string username, string password);

    string GenerateToken(string username);
}

