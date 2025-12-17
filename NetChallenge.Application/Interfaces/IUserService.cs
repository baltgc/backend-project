using NetChallenge.Application.DTOs;

namespace NetChallenge.Application.Interfaces;

public interface IUserService
{
    Task<IEnumerable<UserDto>> GetUsersAsync();

    Task<UserDto?> GetUserByIdAsync(int id);
}

