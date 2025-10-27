namespace QuestionBank.Domain.Entities;

/// <summary>
/// 试卷
/// </summary>
public class Paper : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int TotalScore { get; set; } // 总分
    public int Duration { get; set; } // 答题时长(分钟)
    public Guid CreatorId { get; set; }

    // Navigation properties
    public User Creator { get; set; } = null!;
    public ICollection<PaperQuestion> PaperQuestions { get; set; } = new List<PaperQuestion>();
    public ICollection<Exam> Exams { get; set; } = new List<Exam>();
}
