using QuestionBank.Domain.Enums;

namespace QuestionBank.Application.DTOs;

// 试卷相关DTO
public class PaperDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int TotalScore { get; set; }
    public int Duration { get; set; }
    public int QuestionCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreatePaperDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Duration { get; set; }
    public List<PaperQuestionDto> Questions { get; set; } = new();
}

public class PaperQuestionDto
{
    public Guid QuestionId { get; set; }
    public int QuestionOrder { get; set; }
    public int Score { get; set; }
}

public class PaperDetailDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int TotalScore { get; set; }
    public int Duration { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<PaperQuestionDetailDto> Questions { get; set; } = new();
}

public class PaperQuestionDetailDto
{
    public Guid QuestionId { get; set; }
    public int QuestionOrder { get; set; }
    public int Score { get; set; }
    public QuestionDto Question { get; set; } = null!;
}

// 考试相关DTO
public class ExamDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid PaperId { get; set; }
    public ExamStatus Status { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int Duration { get; set; }
    public int MaxAttempts { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateExamDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid PaperId { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int Duration { get; set; }
    public int MaxAttempts { get; set; } = 1;
    public AnswerDisplayMode AnswerDisplayMode { get; set; }
    public bool AllowPause { get; set; } = false;
    public bool ShuffleQuestions { get; set; } = false;
    public bool ShuffleOptions { get; set; } = false;
    public bool RequireFullScreen { get; set; } = false;
    public bool DisableCopyPaste { get; set; } = true;
}

// 考试答题相关DTO
public class ExamAttemptDto
{
    public Guid Id { get; set; }
    public Guid ExamId { get; set; }
    public string ExamTitle { get; set; } = string.Empty;
    public int AttemptNumber { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? SubmitTime { get; set; }
    public int? TotalScore { get; set; }
    public bool IsSubmitted { get; set; }
    public bool IsGraded { get; set; }
}

public class StartExamDto
{
    public Guid ExamId { get; set; }
}

public class SubmitAnswerDto
{
    public Guid ExamAttemptId { get; set; }
    public Guid QuestionId { get; set; }
    public string UserAnswer { get; set; } = string.Empty;
}

public class SubmitExamDto
{
    public Guid ExamAttemptId { get; set; }
}

public class GradeAnswerDto
{
    public Guid AnswerId { get; set; }
    public int Score { get; set; }
    public string? TeacherComment { get; set; }
}
