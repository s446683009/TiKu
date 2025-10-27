using QuestionBank.Application.DTOs;
using QuestionBank.Domain.Enums;

namespace QuestionBank.Application.Services;

public interface IUserService
{
    Task<ApiResponse<UserDto>> GetUserByIdAsync(Guid id);
    Task<ApiResponse<UserDto>> GetUserByUsernameAsync(string username);
    Task<ApiResponse<UserDto>> GetUserByEmailAsync(string email);
    Task<ApiResponse<PagedResponse<UserDto>>> GetAllUsersAsync(int pageNumber = 1, int pageSize = 20);
    Task<ApiResponse<List<UserDto>>> GetUsersByRoleAsync(UserRole role);
    Task<ApiResponse<UserDto>> UpdateUserAsync(UpdateUserDto updateDto);
    Task<ApiResponse<bool>> ChangePasswordAsync(Guid userId, string oldPassword, string newPassword);
    Task<ApiResponse<bool>> ActivateUserAsync(Guid userId);
    Task<ApiResponse<bool>> DeactivateUserAsync(Guid userId);
    Task<ApiResponse<UserStatsDto>> GetUserStatsAsync(Guid userId);
    Task<ApiResponse<SystemStatsDto>> GetSystemStatsAsync();
}
