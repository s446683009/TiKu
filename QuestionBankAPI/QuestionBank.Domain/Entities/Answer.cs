namespace QuestionBank.Domain.Entities;

/// <summary>
/// 答题记录
/// </summary>
public class Answer : BaseEntity
{
    public Guid ExamAttemptId { get; set; }
    public Guid QuestionId { get; set; }
    public string? UserAnswer { get; set; } // 用户答案
    public int? Score { get; set; } // 得分
    public bool? IsCorrect { get; set; } // 是否正确
    public string? TeacherComment { get; set; } // 教师评语
    public Guid? GradedBy { get; set; } // 批改人

    // Navigation properties
    public ExamAttempt ExamAttempt { get; set; } = null!;
    public Question Question { get; set; } = null!;
    public User? Grader { get; set; }
}
