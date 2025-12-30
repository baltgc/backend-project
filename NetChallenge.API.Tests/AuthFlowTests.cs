using System.Net;
using System.Net.Http.Json;
using NetChallenge.API.Tests.Infrastructure;
using NetChallenge.Application.DTOs;

namespace NetChallenge.API.Tests;

public class AuthFlowTests(CustomWebApplicationFactory factory)
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
    public async Task Login_ReturnsAccessTokenAndRefreshToken()
    {
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest { Username = "admin", Password = "admin123" }
        );

        await EnsureSuccessOrDumpAsync(response);

        var body = await response.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(body);
        Assert.False(string.IsNullOrWhiteSpace(body.Token));
        Assert.False(string.IsNullOrWhiteSpace(body.RefreshToken));
        Assert.True(body.ExpiresAt > DateTime.UtcNow);
        Assert.True(body.RefreshTokenExpiresAt > DateTime.UtcNow);
    }

    [Fact]
    public async Task Refresh_RotatesToken_OldRefreshStopsWorking()
    {
        using var client = factory.CreateClient();

        var loginResponse = await (
            await client.PostAsJsonAsync(
                "/api/auth/login",
                new LoginRequest { Username = "admin", Password = "admin123" }
            )
        ).Content.ReadFromJsonAsync<LoginResponse>();

        Assert.NotNull(loginResponse);

        var oldRefresh = loginResponse.RefreshToken;

        var refresh1 = await client.PostAsJsonAsync(
            "/api/auth/refresh",
            new RefreshTokenRequest { RefreshToken = oldRefresh }
        );
        refresh1.EnsureSuccessStatusCode();

        var refresh1Body = await refresh1.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(refresh1Body);
        Assert.False(string.IsNullOrWhiteSpace(refresh1Body.RefreshToken));
        Assert.NotEqual(oldRefresh, refresh1Body.RefreshToken);

        // Old refresh token should now be revoked => 401
        var refreshOldAgain = await client.PostAsJsonAsync(
            "/api/auth/refresh",
            new RefreshTokenRequest { RefreshToken = oldRefresh }
        );
        Assert.Equal(HttpStatusCode.Unauthorized, refreshOldAgain.StatusCode);
    }
}
