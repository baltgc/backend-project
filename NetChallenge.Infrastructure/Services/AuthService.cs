using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NetChallenge.Application.DTOs;
using NetChallenge.Application.Interfaces;
using NetChallenge.Domain.Entities;
using NetChallenge.Infrastructure.Persistence;
using NetChallenge.Infrastructure.Security;

namespace NetChallenge.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly string _secretKey;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _accessTokenExpirationMinutes;
    private readonly int _refreshTokenExpirationDays;
    private readonly AppDbContext _db;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthService(
        string secretKey,
        string issuer,
        string audience,
        int accessTokenExpirationMinutes,
        int refreshTokenExpirationDays,
        AppDbContext db,
        IHttpContextAccessor httpContextAccessor
    )
    {
        _secretKey = secretKey;
        _issuer = issuer;
        _audience = audience;
        _accessTokenExpirationMinutes = accessTokenExpirationMinutes;
        _refreshTokenExpirationDays = refreshTokenExpirationDays;
        _db = db;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<LoginResponse?> AuthenticateAsync(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            return null;
        }

        var user = await _db.UserAccounts.SingleOrDefaultAsync(u => u.Username == username);
        if (user == null || !user.IsActive)
        {
            return null;
        }

        if (!PasswordHasher.VerifyPassword(password, user.PasswordHash, user.PasswordSalt))
        {
            return null;
        }

        user.LastLoginAt = DateTime.UtcNow;

        var (refreshTokenPlaintext, refreshToken) = CreateRefreshToken(user.Id);
        _db.RefreshTokens.Add(refreshToken);
        await _db.SaveChangesAsync();

        var accessToken = GenerateAccessToken(user.Id, user.Username);

        return new LoginResponse
        {
            Token = accessToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_accessTokenExpirationMinutes),
            RefreshToken = refreshTokenPlaintext,
            RefreshTokenExpiresAt = refreshToken.ExpiresAt,
        };
    }

    public async Task<LoginResponse?> RefreshAsync(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return null;
        }

        var now = DateTime.UtcNow;
        var tokenHash = Sha256Base64(refreshToken);

        var existing = await _db.RefreshTokens
            .Include(t => t.UserAccount)
            .SingleOrDefaultAsync(t => t.TokenHash == tokenHash);

        if (
            existing == null
            || existing.RevokedAt != null
            || existing.ExpiresAt <= now
            || existing.UserAccount == null
            || !existing.UserAccount.IsActive
        )
        {
            return null;
        }

        // Rotate refresh token
        var (newPlaintext, newToken) = CreateRefreshToken(existing.UserAccountId);
        existing.RevokedAt = now;
        existing.ReplacedByTokenId = newToken.Id;
        _db.RefreshTokens.Add(newToken);

        await _db.SaveChangesAsync();

        var accessToken = GenerateAccessToken(existing.UserAccount.Id, existing.UserAccount.Username);

        return new LoginResponse
        {
            Token = accessToken,
            ExpiresAt = now.AddMinutes(_accessTokenExpirationMinutes),
            RefreshToken = newPlaintext,
            RefreshTokenExpiresAt = newToken.ExpiresAt,
        };
    }

    public async Task RevokeAsync(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return;
        }

        var tokenHash = Sha256Base64(refreshToken);
        var existing = await _db.RefreshTokens.SingleOrDefaultAsync(t => t.TokenHash == tokenHash);
        if (existing == null || existing.RevokedAt != null)
        {
            return;
        }

        existing.RevokedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    private string GenerateAccessToken(Guid userId, string username)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Name, username),
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_accessTokenExpirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private (string plaintext, RefreshToken token) CreateRefreshToken(Guid userAccountId)
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        var plaintext = Base64UrlEncode(bytes);

        var httpContext = _httpContextAccessor.HttpContext;
        var ip = httpContext?.Connection.RemoteIpAddress?.ToString();
        var userAgent = httpContext?.Request.Headers.UserAgent.ToString();

        var now = DateTime.UtcNow;

        var token = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserAccountId = userAccountId,
            TokenHash = Sha256Base64(plaintext),
            CreatedAt = now,
            ExpiresAt = now.AddDays(_refreshTokenExpirationDays),
            CreatedByIp = ip,
            CreatedByUserAgent = userAgent,
        };

        return (plaintext, token);
    }

    private static string Base64UrlEncode(byte[] bytes)
    {
        // RFC 4648 base64url without padding.
        return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    private static string Sha256Base64(string input)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = SHA256.HashData(bytes);
        return Convert.ToBase64String(hash);
    }
}
