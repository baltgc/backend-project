using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using NetChallenge.Application.DTOs;
using NetChallenge.Application.Interfaces;

namespace NetChallenge.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly string _secretKey;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _expirationMinutes;
    private readonly string _validUsername;
    private readonly string _validPassword;

    public AuthService(
        string secretKey,
        string issuer,
        string audience,
        int expirationMinutes,
        string validUsername,
        string validPassword
    )
    {
        _secretKey = secretKey;
        _issuer = issuer;
        _audience = audience;
        _expirationMinutes = expirationMinutes;
        _validUsername = validUsername;
        _validPassword = validPassword;
    }

    public Task<LoginResponse?> AuthenticateAsync(string username, string password)
    {
        // Validate credentials against hardcoded user
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            return Task.FromResult<LoginResponse?>(null);
        }

        // Only accept the configured valid user
        if (username != _validUsername || password != _validPassword)
        {
            return Task.FromResult<LoginResponse?>(null);
        }

        var token = GenerateToken(username);
        var expiresAt = DateTime.UtcNow.AddMinutes(_expirationMinutes);

        return Task.FromResult<LoginResponse?>(
            new LoginResponse { Token = token, ExpiresAt = expiresAt }
        );
    }

    public string GenerateToken(string username)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, username),
            new Claim(JwtRegisteredClaimNames.Sub, username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_expirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
