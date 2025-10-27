using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuestionBank.Application.DTOs;
using QuestionBank.Application.Services;
using QuestionBank.Domain.Enums;
using System.Security.Claims;

namespace QuestionBank.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// 获取当前登录用户信息
    /// </summary>
    [HttpGet("me")]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetCurrentUser()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _userService.GetUserByIdAsync(userId);

        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// 获取用户详情(管理员)
    /// </summary>
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetUser(Guid id)
    {
        var result = await _userService.GetUserByIdAsync(id);

        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// 获取所有用户(分页)(管理员)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<PagedResponse<UserDto>>>> GetAllUsers(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _userService.GetAllUsersAsync(pageNumber, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// 根据角色获取用户列表(管理员)
    /// </summary>
    [HttpGet("by-role/{role}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<List<UserDto>>>> GetUsersByRole(UserRole role)
    {
        var result = await _userService.GetUsersByRoleAsync(role);
        return Ok(result);
    }

    /// <summary>
    /// 更新用户信息
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<UserDto>>> UpdateUser(Guid id, [FromBody] UpdateUserDto updateDto)
    {
        var currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var currentUserRole = User.FindFirstValue(ClaimTypes.Role);

        // 只能修改自己的信息,除非是管理员
        if (id != currentUserId && currentUserRole != "Admin")
        {
            return Forbid();
        }

        if (id != updateDto.Id)
        {
            return BadRequest(ApiResponse<UserDto>.ErrorResponse("ID不匹配"));
        }

        var result = await _userService.UpdateUserAsync(updateDto);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// 修改密码
    /// </summary>
    [HttpPost("{id}/change-password")]
    public async Task<ActionResult<ApiResponse<bool>>> ChangePassword(
        Guid id,
        [FromBody] ChangePasswordDto changePasswordDto)
    {
        var currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // 只能修改自己的密码
        if (id != currentUserId)
        {
            return Forbid();
        }

        var result = await _userService.ChangePasswordAsync(
            id,
            changePasswordDto.OldPassword,
            changePasswordDto.NewPassword);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// 激活用户(管理员)
    /// </summary>
    [HttpPost("{id}/activate")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<bool>>> ActivateUser(Guid id)
    {
        var result = await _userService.ActivateUserAsync(id);

        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// 禁用用户(管理员)
    /// </summary>
    [HttpPost("{id}/deactivate")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<bool>>> DeactivateUser(Guid id)
    {
        var result = await _userService.DeactivateUserAsync(id);

        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// 获取用户统计数据
    /// </summary>
    [HttpGet("{id}/stats")]
    public async Task<ActionResult<ApiResponse<UserStatsDto>>> GetUserStats(Guid id)
    {
        var currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var currentUserRole = User.FindFirstValue(ClaimTypes.Role);

        // 只能查看自己的统计,除非是教师或管理员
        if (id != currentUserId && currentUserRole != "Teacher" && currentUserRole != "Admin")
        {
            return Forbid();
        }

        var result = await _userService.GetUserStatsAsync(id);

        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// 获取系统统计数据(管理员)
    /// </summary>
    [HttpGet("system/stats")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<SystemStatsDto>>> GetSystemStats()
    {
        var result = await _userService.GetSystemStatsAsync();
        return Ok(result);
    }
}
