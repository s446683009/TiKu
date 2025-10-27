using QuestionBank.Domain.Enums;

namespace QuestionBank.Domain.Entities;

/// <summary>
/// 考试/练习
/// </summary>
public class Exam : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid PaperId { get; set; }
    public Guid CreatorId { get; set; }
    public ExamStatus Status { get; set; } = ExamStatus.Draft;
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int Duration { get; set; } // 答题时长(分钟)
    public int MaxAttempts { get; set; } = 1; // 允许参考次数
    public AnswerDisplayMode AnswerDisplayMode { get; set; } = AnswerDisplayMode.AfterSubmit;
    public bool AllowPause { get; set; } = false; // 是否允许暂停
    public bool ShuffleQuestions { get; set; } = false; // 题目乱序
    public bool ShuffleOptions { get; set; } = false; // 选项乱序
    public bool RequireFullScreen { get; set; } = false; // 全屏模式
    public bool DisableCopyPaste { get; set; } = true; // 禁止复制粘贴

    // Navigation properties
    public Paper Paper { get; set; } = null!;
    public User Creator { get; set; } = null!;
    public ICollection<ExamAttempt> ExamAttempts { get; set; } = new List<ExamAttempt>();
}
