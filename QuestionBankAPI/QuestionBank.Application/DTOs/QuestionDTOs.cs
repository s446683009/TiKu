using QuestionBank.Domain.Enums;

namespace QuestionBank.Application.DTOs;

// 题目相关DTO
public class QuestionDto
{
    public Guid Id { get; set; }
    public QuestionType Type { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? Options { get; set; }
    public string CorrectAnswer { get; set; } = string.Empty;
    public string? Explanation { get; set; }
    public DifficultyLevel Difficulty { get; set; }
    public int Score { get; set; }
    public string? Chapter { get; set; }
    public QuestionStatus Status { get; set; }
    public List<KnowledgePointDto> KnowledgePoints { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class CreateQuestionDto
{
    public QuestionType Type { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? Options { get; set; }
    public string CorrectAnswer { get; set; } = string.Empty;
    public string? Explanation { get; set; }
    public DifficultyLevel Difficulty { get; set; }
    public int Score { get; set; } = 1;
    public string? Chapter { get; set; }
    public List<Guid> KnowledgePointIds { get; set; } = new();
}

public class UpdateQuestionDto
{
    public Guid Id { get; set; }
    public QuestionType Type { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? Options { get; set; }
    public string CorrectAnswer { get; set; } = string.Empty;
    public string? Explanation { get; set; }
    public DifficultyLevel Difficulty { get; set; }
    public int Score { get; set; }
    public string? Chapter { get; set; }
    public QuestionStatus Status { get; set; }
    public List<Guid> KnowledgePointIds { get; set; } = new();
}

public class QuestionSearchDto
{
    public string? Keyword { get; set; }
    public QuestionType? Type { get; set; }
    public DifficultyLevel? Difficulty { get; set; }
    public Guid? KnowledgePointId { get; set; }
    public string? Chapter { get; set; }
    public QuestionStatus? Status { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

// 知识点相关DTO
public class KnowledgePointDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? ParentId { get; set; }
    public int Level { get; set; }
    public List<KnowledgePointDto> Children { get; set; } = new();
}

public class CreateKnowledgePointDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? ParentId { get; set; }
}

// 批量导入相关DTO
public class QuestionImportDto
{
    public QuestionType Type { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? Options { get; set; }
    public string CorrectAnswer { get; set; } = string.Empty;
    public string? Explanation { get; set; }
    public DifficultyLevel Difficulty { get; set; } = DifficultyLevel.Medium;
    public int Score { get; set; } = 1;
    public string? Chapter { get; set; }
    public List<string> KnowledgePointNames { get; set; } = new();
    public int LineNumber { get; set; } // 用于错误报告
}

public class QuestionImportResultDto
{
    public int TotalCount { get; set; }
    public int SuccessCount { get; set; }
    public int FailedCount { get; set; }
    public List<QuestionImportErrorDto> Errors { get; set; } = new();
    public List<Guid> ImportedQuestionIds { get; set; } = new();
}

public class QuestionImportErrorDto
{
    public int LineNumber { get; set; }
    public string QuestionContent { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
}
