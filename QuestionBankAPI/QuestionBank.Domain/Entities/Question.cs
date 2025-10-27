using QuestionBank.Domain.Enums;

namespace QuestionBank.Domain.Entities;

/// <summary>
/// 题目实体
/// </summary>
public class Question : BaseEntity
{
    public QuestionType Type { get; set; }
    public string Content { get; set; } = string.Empty; // 题干(富文本)
    public string? Options { get; set; } // 选项(JSON格式存储,仅客观题使用)
    public string CorrectAnswer { get; set; } = string.Empty; // 正确答案
    public string? Explanation { get; set; } // 解析(富文本)
    public DifficultyLevel Difficulty { get; set; } = DifficultyLevel.Medium;
    public int Score { get; set; } = 1; // 题目分值
    public string? Chapter { get; set; } // 所属章节
    public QuestionStatus Status { get; set; } = QuestionStatus.Enabled;
    public Guid? CreatorId { get; set; } // 出题人
    public Guid? MaterialQuestionId { get; set; } // 材料题的母题ID
    public int? SubQuestionOrder { get; set; } // 材料题下小题的顺序

    // Navigation properties
    public User? Creator { get; set; }
    public Question? MaterialQuestion { get; set; } // 材料题母题
    public ICollection<Question> SubQuestions { get; set; } = new List<Question>(); // 材料题下的小题
    public ICollection<QuestionKnowledgePoint> QuestionKnowledgePoints { get; set; } = new List<QuestionKnowledgePoint>();
    public ICollection<PaperQuestion> PaperQuestions { get; set; } = new List<PaperQuestion>();
    public ICollection<Answer> Answers { get; set; } = new List<Answer>();
}
