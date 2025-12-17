using NetChallenge.Application.DTOs;
using NetChallenge.Application.Interfaces;

namespace NetChallenge.Application.UseCases;

public class GetUserByIdUseCase
{
    private readonly IUserService _userService;

    public GetUserByIdUseCase(IUserService userService)
    {
        _userService = userService;
    }

    public async Task<UserDto?> ExecuteAsync(int id)
    {
        return await _userService.GetUserByIdAsync(id);
    }
}

