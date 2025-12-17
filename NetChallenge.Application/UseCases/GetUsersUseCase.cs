using NetChallenge.Application.DTOs;
using NetChallenge.Application.Interfaces;

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
        return await _userService.GetUsersAsync();
    }
}

