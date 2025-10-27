namespace QuestionBank.Domain.Entities;

/// <summary>
/// 试卷-题目关联表 (多对多)
/// </summary>
public class PaperQuestion
{
    public Guid PaperId { get; set; }
    public Guid QuestionId { get; set; }
    public int QuestionOrder { get; set; } // 题目在试卷中的顺序
    public int Score { get; set; } // 该题在此试卷中的分值

    // Navigation properties
    public Paper Paper { get; set; } = null!;
    public Question Question { get; set; } = null!;
}
