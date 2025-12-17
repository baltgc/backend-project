using NetChallenge.Application.DTOs;
using NetChallenge.Application.Interfaces;
using NetChallenge.Domain.Entities;

namespace NetChallenge.Application.UseCases;

public class GetUsersUseCase
{
    private readonly IUserService _userService;

    public GetUsersUseCase(IUserService userService)
    {
        _userService = userService;
    }

    public async Task<IEnumerable<UserDto>> ExecuteAsync()
    {
        var users = await _userService.GetUsersAsync();

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
}

