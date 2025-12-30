using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using NetChallenge.API.Configuration;
using NetChallenge.API.Middleware;
using NetChallenge.Application.Interfaces;
using NetChallenge.Application.UseCases;
using NetChallenge.Infrastructure.External;
using NetChallenge.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

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

builder.Services.AddHttpClient();
builder.Services.AddScoped<JsonPlaceholderClient>(sp =>
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient();
    return new JsonPlaceholderClient(httpClient, jsonPlaceholderBaseUrl);
});

// Register Application Services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService>(sp =>
{
    var validUsername = builder.Configuration["Authentication:ValidUsername"] ?? "admin";
    var validPassword = builder.Configuration["Authentication:ValidPassword"] ?? "admin123";

    return new AuthService(
        jwtSettings.SecretKey,
        jwtSettings.Issuer,
        jwtSettings.Audience,
        jwtSettings.ExpirationMinutes,
        validUsername,
        validPassword
    );
});

// Register Use Cases
builder.Services.AddScoped<GetUsersUseCase>();
builder.Services.AddScoped<GetUserByIdUseCase>();
builder.Services.AddScoped<LoginUseCase>();

var app = builder.Build();

// Global exception handler (first in pipeline)
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
else
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
