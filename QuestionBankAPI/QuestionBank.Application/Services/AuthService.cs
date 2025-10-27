using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using QuestionBank.Application.DTOs;
using QuestionBank.Domain.Entities;
using QuestionBank.Infrastructure.Repositories;
using BCrypt.Net;

namespace QuestionBank.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;

    public AuthService(IUnitOfWork unitOfWork, IConfiguration configuration)
    {
        _unitOfWork = unitOfWork;
        _configuration = configuration;
    }

    public async Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginDto loginDto)
    {
        var users = await _unitOfWork.Repository<User>()
            .FindAsync(u => u.Username == loginDto.Username);
        var user = users.FirstOrDefault();

        if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
        {
            return ApiResponse<LoginResponseDto>.ErrorResponse("用户名或密码错误");
        }

        if (!user.IsActive)
        {
            return ApiResponse<LoginResponseDto>.ErrorResponse("账户已被禁用");
        }

        // 更新最后登录时间
        user.LastLoginAt = DateTime.UtcNow;
        await _unitOfWork.Repository<User>().UpdateAsync(user);

        var token = GenerateJwtToken(user);

        var response = new LoginResponseDto
        {
            Token = token,
            User = MapToUserDto(user)
        };

        return ApiResponse<LoginResponseDto>.SuccessResponse(response, "登录成功");
    }

    public async Task<ApiResponse<UserDto>> RegisterAsync(RegisterDto registerDto)
    {
        // 检查用户名是否已存在
        var existingUser = await _unitOfWork.Repository<User>()
            .ExistsAsync(u => u.Username == registerDto.Username);
        if (existingUser)
        {
            return ApiResponse<UserDto>.ErrorResponse("用户名已存在");
        }

        // 检查邮箱是否已存在
        var existingEmail = await _unitOfWork.Repository<User>()
            .ExistsAsync(u => u.Email == registerDto.Email);
        if (existingEmail)
        {
            return ApiResponse<UserDto>.ErrorResponse("邮箱已被注册");
        }

        var user = new User
        {
            Username = registerDto.Username,
            Email = registerDto.Email,
            FullName = registerDto.FullName,
            Phone = registerDto.Phone,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
            Role = Domain.Enums.UserRole.Student, // 默认为学员
            IsActive = true
        };

        await _unitOfWork.Repository<User>().AddAsync(user);

        return ApiResponse<UserDto>.SuccessResponse(MapToUserDto(user), "注册成功");
    }

    public async Task<ApiResponse<UserDto>> GetCurrentUserAsync(Guid userId)
    {
        var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId);
        if (user == null)
        {
            return ApiResponse<UserDto>.ErrorResponse("用户不存在");
        }

        return ApiResponse<UserDto>.SuccessResponse(MapToUserDto(user));
    }

    private string GenerateJwtToken(User user)
    {
        var securityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "YourSuperSecretKeyHere123456789012"));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"] ?? "QuestionBankAPI",
            audience: _configuration["Jwt:Audience"] ?? "QuestionBankClient",
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private UserDto MapToUserDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            FullName = user.FullName,
            Role = user.Role,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };
    }
}
