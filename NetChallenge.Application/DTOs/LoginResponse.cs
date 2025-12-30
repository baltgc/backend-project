namespace NetChallenge.Application.DTOs;

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }

    public string RefreshToken { get; set; } = string.Empty;

    public DateTime RefreshTokenExpiresAt { get; set; }
}
