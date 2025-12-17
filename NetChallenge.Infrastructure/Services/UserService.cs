using NetChallenge.Application.Interfaces;
using NetChallenge.Domain.Entities;
using NetChallenge.Infrastructure.External;

namespace NetChallenge.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly JsonPlaceholderClient _jsonPlaceholderClient;

    public UserService(JsonPlaceholderClient jsonPlaceholderClient)
    {
        _jsonPlaceholderClient = jsonPlaceholderClient;
    }

    public async Task<IEnumerable<User>> GetUsersAsync()
    {
        var users = await _jsonPlaceholderClient.GetUsersAsync();

        if (users == null)
        {
            return Enumerable.Empty<User>();
        }

        return users.Select(u => new User
        {
            Id = u.Id,
            Name = u.Name,
            Username = u.Username,
            Email = u.Email,
            Phone = u.Phone,
            Website = u.Website
        });
    }

    public async Task<User?> GetUserByIdAsync(int id)
    {
        var user = await _jsonPlaceholderClient.GetUserByIdAsync(id);

        if (user == null)
        {
            return null;
        }

        return new User
        {
            Id = user.Id,
            Name = user.Name,
            Username = user.Username,
            Email = user.Email,
            Phone = user.Phone,
            Website = user.Website
        };
    }
}

