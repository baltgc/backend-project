namespace NetChallenge.Domain.Entities;

public class RefreshToken
{
    public Guid Id { get; set; }

    public Guid UserAccountId { get; set; }

    public UserAccount? UserAccount { get; set; }

    /// <summary>
    /// SHA-256 hash (Base64) of the plaintext refresh token presented by clients.
    /// </summary>
    public string TokenHash { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? RevokedAt { get; set; }

    public Guid? ReplacedByTokenId { get; set; }

    public RefreshToken? ReplacedByToken { get; set; }

    public string? CreatedByIp { get; set; }

    public string? CreatedByUserAgent { get; set; }
}
