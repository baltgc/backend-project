using NetChallenge.Application.DTOs;
using NetChallenge.Application.Interfaces;
using NetChallenge.Infrastructure.External;

namespace NetChallenge.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly JsonPlaceholderClient _jsonPlaceholderClient;

    public UserService(JsonPlaceholderClient jsonPlaceholderClient)
    {
        _jsonPlaceholderClient = jsonPlaceholderClient;
    }

    public async Task<IEnumerable<UserDto>> GetUsersAsync()
    {
        var users = await _jsonPlaceholderClient.GetUsersAsync();

        if (users == null)
        {
            return Enumerable.Empty<UserDto>();
        }

        return users.Select(u => new UserDto
        {
            Id = u.Id,
            Name = u.Name,
            Username = u.Username,
            Email = u.Email,
            Phone = u.Phone,
            Website = u.Website
        });
    }

    public async Task<UserDto?> GetUserByIdAsync(int id)
    {
        var user = await _jsonPlaceholderClient.GetUserByIdAsync(id);

        if (user == null)
        {
            return null;
        }

        return new UserDto
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

