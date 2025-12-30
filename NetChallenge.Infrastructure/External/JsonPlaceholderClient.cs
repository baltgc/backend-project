using System.Text.Json;

namespace NetChallenge.Infrastructure.External;

public class JsonPlaceholderClient
{
    private readonly HttpClient _httpClient;

    public JsonPlaceholderClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<JsonPlaceholderUserResponse>?> GetUsersAsync()
    {
        return await GetAsync<List<JsonPlaceholderUserResponse>>("users");
    }

    public async Task<JsonPlaceholderUserResponse?> GetUserByIdAsync(int id)
    {
        return await GetAsync<JsonPlaceholderUserResponse>($"users/{id}");
    }

    private async Task<T?> GetAsync<T>(string url)
    {
        using var response = await _httpClient.GetAsync(url);

        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new ExternalServiceException(
                message: $"JsonPlaceholder call failed with status {(int)response.StatusCode}.",
                statusCode: (int)response.StatusCode
            );
        }

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        return JsonSerializer.Deserialize<T>(content, options);
    }
}
