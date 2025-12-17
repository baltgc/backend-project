using System.Text.Json;

namespace NetChallenge.Infrastructure.External;

public class JsonPlaceholderClient
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    public JsonPlaceholderClient(HttpClient httpClient, string baseUrl)
    {
        _httpClient = httpClient;
        _baseUrl = baseUrl;
    }

    public async Task<List<JsonPlaceholderUserResponse>?> GetUsersAsync()
    {
        var url = $"{_baseUrl}/users";
        return await GetAsync<List<JsonPlaceholderUserResponse>>(url);
    }

    public async Task<JsonPlaceholderUserResponse?> GetUserByIdAsync(int id)
    {
        var url = $"{_baseUrl}/users/{id}";
        return await GetAsync<JsonPlaceholderUserResponse>(url);
    }

    private async Task<T?> GetAsync<T>(string url)
    {
        var response = await _httpClient.GetAsync(url);

        if (!response.IsSuccessStatusCode)
        {
            return default;
        }

        var content = await response.Content.ReadAsStringAsync();

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        
        return JsonSerializer.Deserialize<T>(content, options);
    }
}

