using QuestionBank.Application.DTOs;

namespace QuestionBank.Application.Services;

public interface IAuthService
{
    Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginDto loginDto);
    Task<ApiResponse<UserDto>> RegisterAsync(RegisterDto registerDto);
    Task<ApiResponse<UserDto>> GetCurrentUserAsync(Guid userId);
}
