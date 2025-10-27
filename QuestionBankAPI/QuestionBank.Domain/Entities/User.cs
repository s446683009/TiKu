using QuestionBank.Domain.Enums;

namespace QuestionBank.Domain.Entities;

public class User : BaseEntity
{
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string FullName { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Student;
    public bool IsActive { get; set; } = true;
    public DateTime? LastLoginAt { get; set; }

    // Navigation properties
    public ICollection<ExamAttempt> ExamAttempts { get; set; } = new List<ExamAttempt>();
    public ICollection<WrongQuestion> WrongQuestions { get; set; } = new List<WrongQuestion>();
    public ICollection<FavoriteQuestion> FavoriteQuestions { get; set; } = new List<FavoriteQuestion>();
    public ICollection<QuestionNote> QuestionNotes { get; set; } = new List<QuestionNote>();
}
