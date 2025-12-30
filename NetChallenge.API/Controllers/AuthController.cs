using Microsoft.AspNetCore.Mvc;
using NetChallenge.Application.DTOs;
using NetChallenge.Application.UseCases;

namespace NetChallenge.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly LoginUseCase _loginUseCase;
    private readonly RefreshTokenUseCase _refreshTokenUseCase;
    private readonly LogoutUseCase _logoutUseCase;

    public AuthController(
        LoginUseCase loginUseCase,
        RefreshTokenUseCase refreshTokenUseCase,
        LogoutUseCase logoutUseCase
    )
    {
        _loginUseCase = loginUseCase;
        _refreshTokenUseCase = refreshTokenUseCase;
        _logoutUseCase = logoutUseCase;
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        var response = await _loginUseCase.ExecuteAsync(request.Username, request.Password);

        if (response == null)
        {
            return Unauthorized();
        }

        return Ok(response);
    }

    [HttpPost("refresh")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponse>> Refresh([FromBody] RefreshTokenRequest request)
    {
        var response = await _refreshTokenUseCase.ExecuteAsync(request.RefreshToken);

        if (response == null)
        {
            return Unauthorized();
        }

        return Ok(response);
    }

    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request)
    {
        await _logoutUseCase.ExecuteAsync(request.RefreshToken);
        return NoContent();
    }
}
