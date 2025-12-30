namespace NetChallenge.Domain.Entities;

public class CacheEntry
{
    public Guid Id { get; set; }

    /// <summary>
    /// Logical cache key (e.g. "users:all" or "users:1").
    /// </summary>
    public string Key { get; set; } = string.Empty;

    public string PayloadJson { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime ExpiresAt { get; set; }
}
