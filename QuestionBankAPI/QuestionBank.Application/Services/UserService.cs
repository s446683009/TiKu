using Microsoft.EntityFrameworkCore;
using QuestionBank.Application.DTOs;
using QuestionBank.Domain.Entities;
using QuestionBank.Domain.Enums;
using QuestionBank.Infrastructure.Data;
using QuestionBank.Infrastructure.Repositories;

namespace QuestionBank.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly ApplicationDbContext _context;

    public UserService(IUserRepository userRepository, ApplicationDbContext context)
    {
        _userRepository = userRepository;
        _context = context;
    }

    public async Task<ApiResponse<UserDto>> GetUserByIdAsync(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
        {
            return ApiResponse<UserDto>.ErrorResponse("用户不存在");
        }

        return ApiResponse<UserDto>.SuccessResponse(MapToUserDto(user));
    }

    public async Task<ApiResponse<UserDto>> GetUserByUsernameAsync(string username)
    {
        var user = await _userRepository.GetByUsernameAsync(username);
        if (user == null)
        {
            return ApiResponse<UserDto>.ErrorResponse("用户不存在");
        }

        return ApiResponse<UserDto>.SuccessResponse(MapToUserDto(user));
    }

    public async Task<ApiResponse<UserDto>> GetUserByEmailAsync(string email)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        if (user == null)
        {
            return ApiResponse<UserDto>.ErrorResponse("用户不存在");
        }

        return ApiResponse<UserDto>.SuccessResponse(MapToUserDto(user));
    }

    public async Task<ApiResponse<PagedResponse<UserDto>>> GetAllUsersAsync(int pageNumber = 1, int pageSize = 20)
    {
        var totalCount = await _userRepository.GetTotalUserCountAsync();
        var users = await _context.Users
            .OrderBy(u => u.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var pagedResponse = new PagedResponse<UserDto>
        {
            Items = users.Select(MapToUserDto).ToList(),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        return ApiResponse<PagedResponse<UserDto>>.SuccessResponse(pagedResponse);
    }

    public async Task<ApiResponse<List<UserDto>>> GetUsersByRoleAsync(UserRole role)
    {
        var users = await _userRepository.GetUsersByRoleAsync(role);
        return ApiResponse<List<UserDto>>.SuccessResponse(
            users.Select(MapToUserDto).ToList());
    }

    public async Task<ApiResponse<UserDto>> UpdateUserAsync(UpdateUserDto updateDto)
    {
        var user = await _userRepository.GetByIdAsync(updateDto.Id);
        if (user == null)
        {
            return ApiResponse<UserDto>.ErrorResponse("用户不存在");
        }

        // 检查邮箱是否被其他用户使用
        if (user.Email != updateDto.Email)
        {
            var emailExists = await _context.Users
                .AnyAsync(u => u.Email == updateDto.Email && u.Id != updateDto.Id);
            if (emailExists)
            {
                return ApiResponse<UserDto>.ErrorResponse("该邮箱已被其他用户使用");
            }
        }

        user.Email = updateDto.Email;
        user.FullName = updateDto.FullName;
        user.Phone = updateDto.Phone;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);

        return ApiResponse<UserDto>.SuccessResponse(MapToUserDto(user), "用户信息更新成功");
    }

    public async Task<ApiResponse<bool>> ChangePasswordAsync(Guid userId, string oldPassword, string newPassword)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return ApiResponse<bool>.ErrorResponse("用户不存在");
        }

        // 验证旧密码
        if (!BCrypt.Net.BCrypt.Verify(oldPassword, user.PasswordHash))
        {
            return ApiResponse<bool>.ErrorResponse("原密码不正确");
        }

        // 更新密码
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);

        return ApiResponse<bool>.SuccessResponse(true, "密码修改成功");
    }

    public async Task<ApiResponse<bool>> ActivateUserAsync(Guid userId)
    {
        var result = await _userRepository.ActivateUserAsync(userId);
        if (!result)
        {
            return ApiResponse<bool>.ErrorResponse("用户不存在");
        }

        return ApiResponse<bool>.SuccessResponse(true, "用户已激活");
    }

    public async Task<ApiResponse<bool>> DeactivateUserAsync(Guid userId)
    {
        var result = await _userRepository.DeactivateUserAsync(userId);
        if (!result)
        {
            return ApiResponse<bool>.ErrorResponse("用户不存在");
        }

        return ApiResponse<bool>.SuccessResponse(true, "用户已禁用");
    }

    public async Task<ApiResponse<UserStatsDto>> GetUserStatsAsync(Guid userId)
    {
        var user = await _userRepository.GetUserWithExamAttemptsAsync(userId);
        if (user == null)
        {
            return ApiResponse<UserStatsDto>.ErrorResponse("用户不存在");
        }

        var wrongQuestionCount = await _context.WrongQuestions
            .CountAsync(wq => wq.UserId == userId && !wq.IsRemoved);

        var favoriteCount = await _context.FavoriteQuestions
            .CountAsync(fq => fq.UserId == userId);

        var noteCount = await _context.QuestionNotes
            .CountAsync(qn => qn.UserId == userId);

        var completedExams = user.ExamAttempts.Where(ea => ea.IsSubmitted).ToList();
        var gradedExams = completedExams.Where(ea => ea.IsGraded && ea.TotalScore.HasValue).ToList();

        var stats = new UserStatsDto
        {
            UserId = user.Id,
            Username = user.Username,
            FullName = user.FullName,
            TotalExamAttempts = user.ExamAttempts.Count,
            CompletedExams = completedExams.Count,
            AverageScore = gradedExams.Any() ? gradedExams.Average(e => e.TotalScore!.Value) : 0,
            HighestScore = gradedExams.Any() ? gradedExams.Max(e => e.TotalScore) : null,
            WrongQuestionCount = wrongQuestionCount,
            FavoriteQuestionCount = favoriteCount,
            NoteCount = noteCount,
            LastLoginAt = user.LastLoginAt,
            RegisteredAt = user.CreatedAt
        };

        // 计算答题统计
        var allAnswers = await _context.Answers
            .Include(a => a.ExamAttempt)
            .Where(a => a.ExamAttempt.UserId == userId)
            .ToListAsync();

        stats.TotalQuestionsAnswered = allAnswers.Count;
        stats.CorrectAnswers = allAnswers.Count(a => a.IsCorrect == true);
        stats.AccuracyRate = allAnswers.Any()
            ? (double)stats.CorrectAnswers / stats.TotalQuestionsAnswered * 100
            : 0;

        return ApiResponse<UserStatsDto>.SuccessResponse(stats);
    }

    public async Task<ApiResponse<SystemStatsDto>> GetSystemStatsAsync()
    {
        var stats = new SystemStatsDto
        {
            TotalUsers = await _userRepository.GetTotalUserCountAsync(),
            StudentCount = await _userRepository.GetUserCountByRoleAsync(UserRole.Student),
            TeacherCount = await _userRepository.GetUserCountByRoleAsync(UserRole.Teacher),
            AdminCount = await _userRepository.GetUserCountByRoleAsync(UserRole.Admin),
            ActiveUsers = await _context.Users.CountAsync(u => u.IsActive),

            TotalQuestions = await _context.Questions.CountAsync(),
            TotalExams = await _context.Exams.CountAsync(),
            ActiveExams = await _context.Exams.CountAsync(e => e.Status == ExamStatus.Published || e.Status == ExamStatus.InProgress),
            TotalExamAttempts = await _context.ExamAttempts.CountAsync()
        };

        // 按题型统计
        var questionsByType = await _context.Questions
            .GroupBy(q => q.Type)
            .Select(g => new { Type = g.Key, Count = g.Count() })
            .ToListAsync();

        stats.QuestionsByType = questionsByType.ToDictionary(
            x => x.Type.ToString(),
            x => x.Count
        );

        // 按难度统计
        var questionsByDifficulty = await _context.Questions
            .GroupBy(q => q.Difficulty)
            .Select(g => new { Difficulty = g.Key, Count = g.Count() })
            .ToListAsync();

        stats.QuestionsByDifficulty = questionsByDifficulty.ToDictionary(
            x => x.Difficulty.ToString(),
            x => x.Count
        );

        return ApiResponse<SystemStatsDto>.SuccessResponse(stats);
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
