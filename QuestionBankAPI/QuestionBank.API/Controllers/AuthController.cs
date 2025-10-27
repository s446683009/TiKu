using Microsoft.AspNetCore.Mvc;
using QuestionBank.Application.DTOs;
using QuestionBank.Application.Services;

namespace QuestionBank.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// 用户注册
    /// </summary>
    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<UserDto>>> Register([FromBody] RegisterDto registerDto)
    {
        var result = await _authService.RegisterAsync(registerDto);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// 用户登录
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Login([FromBody] LoginDto loginDto)
    {
        var result = await _authService.LoginAsync(loginDto);
        if (!result.Success)
        {
            return Unauthorized(result);
        }
        return Ok(result);
    }
}
