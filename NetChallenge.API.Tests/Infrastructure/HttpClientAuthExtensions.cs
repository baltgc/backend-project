using System.Net.Http.Headers;

namespace NetChallenge.API.Tests.Infrastructure;

internal static class HttpClientAuthExtensions
{
    public static void SetBearerToken(this HttpClient client, string token)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }
}


