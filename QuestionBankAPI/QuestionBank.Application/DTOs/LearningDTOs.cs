namespace QuestionBank.Application.DTOs;

// 错题本相关DTOs
public class WrongQuestionDto
{
    public Guid Id { get; set; }
    public Guid QuestionId { get; set; }
    public int WrongCount { get; set; }
    public DateTime LastWrongAt { get; set; }
    public QuestionDto Question { get; set; } = null!;
}

public class AddWrongQuestionDto
{
    public Guid QuestionId { get; set; }
}

// 收藏相关DTOs
public class FavoriteQuestionDto
{
    public Guid Id { get; set; }
    public Guid QuestionId { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; }
    public QuestionDto Question { get; set; } = null!;
}

public class AddFavoriteDto
{
    public Guid QuestionId { get; set; }
    public string? Note { get; set; }
}

public class UpdateFavoriteDto
{
    public string? Note { get; set; }
}

// 笔记相关DTOs
public class QuestionNoteDto
{
    public Guid Id { get; set; }
    public Guid QuestionId { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public QuestionDto? Question { get; set; }
}

public class CreateQuestionNoteDto
{
    public Guid QuestionId { get; set; }
    public string Content { get; set; } = string.Empty;
}

public class UpdateQuestionNoteDto
{
    public string Content { get; set; } = string.Empty;
}
