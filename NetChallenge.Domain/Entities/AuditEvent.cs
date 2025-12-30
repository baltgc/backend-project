namespace NetChallenge.Domain.Entities;

public class AuditEvent
{
    public Guid Id { get; set; }

    public string Type { get; set; } = string.Empty;

    public Guid? UserAccountId { get; set; }

    public UserAccount? UserAccount { get; set; }

    public DateTime Timestamp { get; set; }

    public string CorrelationId { get; set; } = string.Empty;

    public string? Path { get; set; }

    public string? Method { get; set; }

    public int? StatusCode { get; set; }

    /// <summary>
    /// JSON blob for small metadata (e.g. cacheHit=true, externalStatus=200).
    /// Keep it intentionally small; this is audit, not a data lake.
    /// </summary>
    public string? MetadataJson { get; set; }
}
