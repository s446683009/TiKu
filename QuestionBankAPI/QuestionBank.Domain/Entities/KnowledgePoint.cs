namespace QuestionBank.Domain.Entities;

/// <summary>
/// 知识点 - 支持多级分类
/// </summary>
public class KnowledgePoint : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? ParentId { get; set; }
    public int Level { get; set; } // 层级,1表示一级知识点
    public int SortOrder { get; set; }

    // Navigation properties
    public KnowledgePoint? Parent { get; set; }
    public ICollection<KnowledgePoint> Children { get; set; } = new List<KnowledgePoint>();
    public ICollection<QuestionKnowledgePoint> QuestionKnowledgePoints { get; set; } = new List<QuestionKnowledgePoint>();
}
