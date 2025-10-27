namespace QuestionBank.Domain.Entities;

/// <summary>
/// 考试答题记录
/// </summary>
public class ExamAttempt : BaseEntity
{
    public Guid ExamId { get; set; }
    public Guid UserId { get; set; }
    public int AttemptNumber { get; set; } // 第几次参考
    public DateTime? StartTime { get; set; }
    public DateTime? SubmitTime { get; set; }
    public int? TotalScore { get; set; } // 得分
    public bool IsSubmitted { get; set; } = false;
    public bool IsGraded { get; set; } = false; // 是否已批改完成

    // Navigation properties
    public Exam Exam { get; set; } = null!;
    public User User { get; set; } = null!;
    public ICollection<Answer> Answers { get; set; } = new List<Answer>();
}
