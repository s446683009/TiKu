using QuestionBank.Domain.Enums;

namespace QuestionBank.Application.DTOs;

// 更新用户信息DTO
public class UpdateUserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
}

// 修改密码DTO
public class ChangePasswordDto
{
    public string OldPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

// 用户统计DTO
public class UserStatsDto
{
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;

    // 考试统计
    public int TotalExamAttempts { get; set; }
    public int CompletedExams { get; set; }
    public double AverageScore { get; set; }
    public int? HighestScore { get; set; }

    // 练习统计
    public int TotalQuestionsAnswered { get; set; }
    public int CorrectAnswers { get; set; }
    public double AccuracyRate { get; set; }

    // 学习记录
    public int WrongQuestionCount { get; set; }
    public int FavoriteQuestionCount { get; set; }
    public int NoteCount { get; set; }

    // 时间统计
    public DateTime? LastLoginAt { get; set; }
    public DateTime RegisteredAt { get; set; }
}

// 系统统计DTO
public class SystemStatsDto
{
    // 用户统计
    public int TotalUsers { get; set; }
    public int StudentCount { get; set; }
    public int TeacherCount { get; set; }
    public int AdminCount { get; set; }
    public int ActiveUsers { get; set; }

    // 题库统计
    public int TotalQuestions { get; set; }
    public Dictionary<string, int> QuestionsByType { get; set; } = new();
    public Dictionary<string, int> QuestionsByDifficulty { get; set; } = new();

    // 考试统计
    public int TotalExams { get; set; }
    public int ActiveExams { get; set; }
    public int TotalExamAttempts { get; set; }

    // 系统信息
    public DateTime ServerTime { get; set; } = DateTime.UtcNow;
}
