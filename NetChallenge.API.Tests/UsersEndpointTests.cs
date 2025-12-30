using System.Net;
using System.Net.Http.Json;
using NetChallenge.API.Middleware;
using NetChallenge.API.Tests.Infrastructure;
using NetChallenge.Application.DTOs;

namespace NetChallenge.API.Tests;

public class UsersEndpointTests(CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>
{
    private static async Task EnsureSuccessOrDumpAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var body = await response.Content.ReadAsStringAsync();
        throw new Xunit.Sdk.XunitException(
            $"HTTP {(int)response.StatusCode} {response.StatusCode}. Body: {body}"
        );
    }

    [Fact]
    public async Task Users_WithoutAuth_Is401_AndStillHasCorrelationHeader()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/users");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.True(response.Headers.Contains(CorrelationIdMiddleware.HeaderName));
    }

    [Fact]
    public async Task Users_WithAuth_ReturnsUsers_AndUsesDbCache()
    {
        using var client = factory.CreateClient();

        // Login
        var login = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest { Username = "admin", Password = "admin123" }
        );
        await EnsureSuccessOrDumpAsync(login);
        var loginBody = await login.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(loginBody);

        client.SetBearerToken(loginBody.Token);

        // First call: should hit fake upstream once and cache in DB.
        var before = factory.JsonPlaceholderHandler.CallCount;
        var r1 = await client.GetAsync("/api/users");
        await EnsureSuccessOrDumpAsync(r1);
        Assert.True(r1.Headers.Contains(CorrelationIdMiddleware.HeaderName));

        var users1 = await r1.Content.ReadFromJsonAsync<List<UserDto>>();
        Assert.NotNull(users1);
        Assert.True(users1.Count > 0);

        // Second call: should be served from DB cache (no additional upstream hits).
        var r2 = await client.GetAsync("/api/users");
        r2.EnsureSuccessStatusCode();

        var after = factory.JsonPlaceholderHandler.CallCount;
        Assert.Equal(before + 1, after);
    }
}
