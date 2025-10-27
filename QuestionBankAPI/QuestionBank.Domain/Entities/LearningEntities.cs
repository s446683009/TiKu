namespace QuestionBank.Domain.Entities;

/// <summary>
/// 错题本
/// </summary>
public class WrongQuestion : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid QuestionId { get; set; }
    public int WrongCount { get; set; } = 1; // 错误次数
    public DateTime LastWrongAt { get; set; } = DateTime.UtcNow;
    public bool IsRemoved { get; set; } = false; // 是否已移除

    // Navigation properties
    public User User { get; set; } = null!;
    public Question Question { get; set; } = null!;
}

/// <summary>
/// 收藏夹
/// </summary>
public class FavoriteQuestion : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid QuestionId { get; set; }
    public string? Note { get; set; } // 收藏备注

    // Navigation properties
    public User User { get; set; } = null!;
    public Question Question { get; set; } = null!;
}

/// <summary>
/// 题目笔记
/// </summary>
public class QuestionNote : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid QuestionId { get; set; }
    public string Content { get; set; } = string.Empty;

    // Navigation properties
    public User User { get; set; } = null!;
    public Question Question { get; set; } = null!;
}
