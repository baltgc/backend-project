using NetChallenge.Application.Interfaces;
using NetChallenge.Application.UseCases;
using NetChallenge.Infrastructure.External;
using NetChallenge.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure HttpClient and JsonPlaceholderClient
var jsonPlaceholderBaseUrl = builder.Configuration["JsonPlaceholder:BaseUrl"] 
    ?? "https://jsonplaceholder.typicode.com";

builder.Services.AddHttpClient();
builder.Services.AddScoped<JsonPlaceholderClient>(sp =>
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient();
    return new JsonPlaceholderClient(httpClient, jsonPlaceholderBaseUrl);
});

// Register Application Services
builder.Services.AddScoped<IUserService, UserService>();

// Register Use Cases
builder.Services.AddScoped<GetUsersUseCase>();
builder.Services.AddScoped<GetUserByIdUseCase>();

var app = builder.Build();

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

app.UseAuthorization();

app.MapControllers();

app.Run();
