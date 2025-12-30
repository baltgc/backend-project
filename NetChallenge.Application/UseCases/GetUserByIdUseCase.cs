using NetChallenge.Application.DTOs;
using NetChallenge.Application.Interfaces;
using NetChallenge.Domain.Entities;

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
        var user = await _userService.GetUserByIdAsync(id);

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
            Website = user.Website,
        };
    }
}
