using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using NetChallenge.Application.Interfaces;
using NetChallenge.Domain.Entities;
using NetChallenge.Infrastructure.External;
using NetChallenge.Infrastructure.Persistence;

namespace NetChallenge.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly JsonPlaceholderClient _jsonPlaceholderClient;
    private readonly AppDbContext _db;
    private readonly int _usersTtlSeconds;

    public UserService(JsonPlaceholderClient jsonPlaceholderClient, AppDbContext db, int usersTtlSeconds)
    {
        _jsonPlaceholderClient = jsonPlaceholderClient;
        _db = db;
        _usersTtlSeconds = usersTtlSeconds;
    }

    public async Task<IEnumerable<User>> GetUsersAsync()
    {
        var cached = await TryGetCacheAsync<List<JsonPlaceholderUserResponse>>("users:all");
        var users = cached ?? await _jsonPlaceholderClient.GetUsersAsync();

        if (users == null)
        {
            return Enumerable.Empty<User>();
        }

        if (cached == null)
        {
            await UpsertCacheAsync("users:all", users);
        }

        return users.Select(u => new User
        {
            Id = u.Id,
            Name = u.Name,
            Username = u.Username,
            Email = u.Email,
            Phone = u.Phone,
            Website = u.Website,
        });
    }

    public async Task<User?> GetUserByIdAsync(int id)
    {
        var key = $"users:{id}";
        var cached = await TryGetCacheAsync<JsonPlaceholderUserResponse>(key);
        var user = cached ?? await _jsonPlaceholderClient.GetUserByIdAsync(id);

        if (user == null)
        {
            return null;
        }

        if (cached == null)
        {
            await UpsertCacheAsync(key, user);
        }

        return new User
        {
            Id = user.Id,
            Name = user.Name,
            Username = user.Username,
            Email = user.Email,
            Phone = user.Phone,
            Website = user.Website,
        };
    }

    private async Task<T?> TryGetCacheAsync<T>(string key)
    {
        var now = DateTime.UtcNow;
        var entry = await _db.CacheEntries.AsNoTracking().SingleOrDefaultAsync(c =>
            c.Key == key && c.ExpiresAt > now
        );

        if (entry == null)
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(
            entry.PayloadJson,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        );
    }

    private async Task UpsertCacheAsync<T>(string key, T value)
    {
        var now = DateTime.UtcNow;
        var expiresAt = now.AddSeconds(Math.Max(1, _usersTtlSeconds));

        var payload = JsonSerializer.Serialize(value);

        var existing = await _db.CacheEntries.SingleOrDefaultAsync(c => c.Key == key);
        if (existing == null)
        {
            _db.CacheEntries.Add(
                new CacheEntry
                {
                    Id = Guid.NewGuid(),
                    Key = key,
                    PayloadJson = payload,
                    CreatedAt = now,
                    ExpiresAt = expiresAt,
                }
            );
        }
        else
        {
            existing.PayloadJson = payload;
            existing.CreatedAt = now;
            existing.ExpiresAt = expiresAt;
        }

        await _db.SaveChangesAsync();
    }
}
