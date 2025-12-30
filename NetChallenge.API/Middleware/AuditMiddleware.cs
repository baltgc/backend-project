using System.Security.Claims;
using System.Text.Json;
using NetChallenge.Domain.Entities;
using NetChallenge.Infrastructure.Persistence;

namespace NetChallenge.API.Middleware;

public class AuditMiddleware
{
    private readonly RequestDelegate _next;

    public AuditMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, AppDbContext db, ILogger<AuditMiddleware> logger)
    {
        try
        {
            await _next(context);
        }
        finally
        {
            try
            {
                var correlationId = CorrelationIdMiddleware.GetCorrelationId(context) ?? context.TraceIdentifier;

                Guid? userAccountId = null;
                var userIdClaim = context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
                if (Guid.TryParse(userIdClaim, out var parsed))
                {
                    userAccountId = parsed;
                }

                var audit = new AuditEvent
                {
                    Id = Guid.NewGuid(),
                    Type = "http_request",
                    Timestamp = DateTime.UtcNow,
                    CorrelationId = correlationId,
                    Path = context.Request.Path.ToString(),
                    Method = context.Request.Method,
                    StatusCode = context.Response.StatusCode,
                    UserAccountId = userAccountId,
                    MetadataJson = JsonSerializer.Serialize(
                        new { userAgent = context.Request.Headers.UserAgent.ToString() }
                    ),
                };

                db.AuditEvents.Add(audit);
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Never break the request because auditing failed.
                logger.LogWarning(ex, "Failed to write audit event.");
            }
        }
    }
}


