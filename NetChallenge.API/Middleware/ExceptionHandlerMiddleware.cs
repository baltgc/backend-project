using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using NetChallenge.Infrastructure.External;

namespace NetChallenge.API.Middleware;

public class ExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlerMiddleware> _logger;

    public ExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlerMiddleware> logger
    )
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title) = exception switch
        {
            ArgumentException => (StatusCodes.Status400BadRequest, "Bad Request"),
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Unauthorized"),
            KeyNotFoundException => (StatusCodes.Status404NotFound, "Not Found"),
            ExternalServiceException => (
                StatusCodes.Status503ServiceUnavailable,
                "Upstream Service Failure"
            ),
            HttpRequestException => (
                StatusCodes.Status503ServiceUnavailable,
                "Upstream Service Failure"
            ),
            _ => (StatusCodes.Status500InternalServerError, "Internal Server Error"),
        };

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = statusCode;

        var correlationId =
            CorrelationIdMiddleware.GetCorrelationId(context) ?? context.TraceIdentifier;

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail =
                context.RequestServices.GetRequiredService<IHostEnvironment>().IsDevelopment()
                || context
                    .RequestServices.GetRequiredService<IHostEnvironment>()
                    .IsEnvironment("Testing")
                    ? exception.Message
                    : null,
            Instance = context.Request.Path,
        };

        problem.Extensions["correlationId"] = correlationId;

        var json = JsonSerializer.Serialize(problem);
        await context.Response.WriteAsync(json);
    }
}
