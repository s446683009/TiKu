namespace QuestionBank.Domain.Entities;

/// <summary>
/// 题目-知识点关联表 (多对多)
/// </summary>
public class QuestionKnowledgePoint
{
    public Guid QuestionId { get; set; }
    public Guid KnowledgePointId { get; set; }

    // Navigation properties
    public Question Question { get; set; } = null!;
    public KnowledgePoint KnowledgePoint { get; set; } = null!;
}
