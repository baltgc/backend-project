using Microsoft.AspNetCore.Mvc;
using NetChallenge.Application.DTOs;
using NetChallenge.Application.UseCases;

namespace NetChallenge.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly GetUsersUseCase _getUsersUseCase;
    private readonly GetUserByIdUseCase _getUserByIdUseCase;

    public UsersController(
        GetUsersUseCase getUsersUseCase,
        GetUserByIdUseCase getUserByIdUseCase)
    {
        _getUsersUseCase = getUsersUseCase;
        _getUserByIdUseCase = getUserByIdUseCase;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<UserDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
    {
        var users = await _getUsersUseCase.ExecuteAsync();
        return Ok(users);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> GetUserById(int id)
    {
        var user = await _getUserByIdUseCase.ExecuteAsync(id);

        if (user == null)
        {
            return NotFound();
        }

        return Ok(user);
    }
}

