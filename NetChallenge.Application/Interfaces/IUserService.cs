using NetChallenge.Domain.Entities;

namespace NetChallenge.Application.Interfaces;

public interface IUserService
{
    Task<IEnumerable<User>> GetUsersAsync();

    Task<User?> GetUserByIdAsync(int id);
}

