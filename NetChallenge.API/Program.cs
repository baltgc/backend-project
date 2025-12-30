using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NetChallenge.API.Configuration;
using NetChallenge.API.Middleware;
using NetChallenge.Application.Interfaces;
using NetChallenge.Application.UseCases;
using NetChallenge.Infrastructure.External;
using NetChallenge.Infrastructure.Persistence;
using NetChallenge.Infrastructure.Services;
using Polly;
using Polly.Extensions.Http;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpLogging(_ => { });

builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = ctx =>
    {
        var correlationId = CorrelationIdMiddleware.GetCorrelationId(ctx.HttpContext);
        if (!string.IsNullOrWhiteSpace(correlationId))
        {
            ctx.ProblemDetails.Extensions["correlationId"] = correlationId;
        }
    };
});

// Configure EF Core (Postgres)
var postgresConnectionString =
    builder.Configuration.GetConnectionString("Postgres")
    ?? throw new InvalidOperationException("Missing connection string: ConnectionStrings:Postgres");
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(postgresConnectionString));
builder.Services.AddHealthChecks().AddDbContextCheck<AppDbContext>();

// Configure JWT Settings
var jwtSettings = new JwtSettings();
builder.Configuration.GetSection("Jwt").Bind(jwtSettings);
builder.Services.AddSingleton(jwtSettings);

// Configure JWT Authentication
var key = Encoding.UTF8.GetBytes(jwtSettings.SecretKey);
builder
    .Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
        };
    });

// Configure Swagger with JWT Authentication
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc(
        "v1",
        new Microsoft.OpenApi.Models.OpenApiInfo
        {
            Title = "NetChallenge API",
            Version = "v1",
            Description =
                "A REST API that consumes data from JSONPlaceholder external API with JWT authentication",
        }
    );

    // Add JWT Authentication to Swagger
    options.AddSecurityDefinition(
        "Bearer",
        new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            Description =
                "Enter your JWT token in the format: {your token}\n\nExample: eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
        }
    );

    options.AddSecurityRequirement(
        new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
        {
            {
                new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Reference = new Microsoft.OpenApi.Models.OpenApiReference
                    {
                        Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                        Id = "Bearer",
                    },
                },
                Array.Empty<string>()
            },
        }
    );
});

// Configure HttpClient and JsonPlaceholderClient
var jsonPlaceholderBaseUrl =
    builder.Configuration["JsonPlaceholder:BaseUrl"] ?? "https://jsonplaceholder.typicode.com";

builder
    .Services.AddHttpClient<JsonPlaceholderClient>(client =>
    {
        client.BaseAddress = new Uri(jsonPlaceholderBaseUrl);
    })
    .AddPolicyHandler(
        HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => (int)msg.StatusCode == StatusCodes.Status429TooManyRequests)
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: attempt => TimeSpan.FromMilliseconds(200 * attempt)
            )
    )
    .AddPolicyHandler(
        HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromSeconds(10)
            )
    )
    .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(3)));

// Register Application Services
builder.Services.AddScoped<IUserService>(sp => new UserService(
    sp.GetRequiredService<JsonPlaceholderClient>(),
    sp.GetRequiredService<AppDbContext>(),
    usersTtlSeconds: builder.Configuration.GetValue("Cache:UsersTtlSeconds", 60)
));
builder.Services.AddScoped<IAuthService>(sp => new AuthService(
    secretKey: jwtSettings.SecretKey,
    issuer: jwtSettings.Issuer,
    audience: jwtSettings.Audience,
    accessTokenExpirationMinutes: jwtSettings.ExpirationMinutes,
    refreshTokenExpirationDays: builder.Configuration.GetValue("Jwt:RefreshTokenExpirationDays", 7),
    db: sp.GetRequiredService<AppDbContext>(),
    httpContextAccessor: sp.GetRequiredService<IHttpContextAccessor>()
));

// Register Use Cases
builder.Services.AddScoped<GetUsersUseCase>();
builder.Services.AddScoped<GetUserByIdUseCase>();
builder.Services.AddScoped<LoginUseCase>();
builder.Services.AddScoped<RefreshTokenUseCase>();
builder.Services.AddScoped<LogoutUseCase>();

var app = builder.Build();

// Development-only: auto-migrate and seed an admin user for convenience.
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();

    var logger = scope
        .ServiceProvider.GetRequiredService<ILoggerFactory>()
        .CreateLogger("DbSeeder");
    var seedUsername = builder.Configuration["Authentication:ValidUsername"] ?? "admin";
    var seedPassword = builder.Configuration["Authentication:ValidPassword"] ?? "admin123";
    await DbSeeder.SeedAdminAsync(db, logger, seedUsername, seedPassword);
}

app.UseHttpLogging();

// Correlation id (first in pipeline)
app.UseMiddleware<CorrelationIdMiddleware>();

// Global exception handler (early in pipeline)
app.UseMiddleware<ExceptionHandlerMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "NetChallenge API v1");
        c.RoutePrefix = string.Empty; // Swagger UI at root
    });
}
else if (app.Environment.IsEnvironment("Testing"))
{
    // Keep the pipeline test-friendly (no HTTPS redirects, no swagger UI required).
}
else
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<AuditMiddleware>();

app.MapHealthChecks("/health");

app.MapControllers();

app.Run();

// Required for WebApplicationFactory<T> in integration tests (top-level statements).
public partial class Program { }
