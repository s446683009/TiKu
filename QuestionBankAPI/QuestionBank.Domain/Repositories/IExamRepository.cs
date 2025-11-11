using QuestionBank.Domain.Entities;
using QuestionBank.Domain.Enums;

namespace QuestionBank.Domain.Repositories;

/// <summary>
/// 考试仓储接口
/// </summary>
public interface IExamRepository : IRepository<Exam>
{
    Task<Exam?> GetExamWithPaperAsync(Guid id);
    Task<Exam?> GetExamWithFullDetailsAsync(Guid id);
    Task<IEnumerable<Exam>> GetExamsByStatusAsync(ExamStatus status);
    Task<IEnumerable<Exam>> GetExamsByCreatorAsync(Guid creatorId);
    Task<IEnumerable<Exam>> GetActiveExamsAsync();
    Task<IEnumerable<Exam>> GetUpcomingExamsAsync();
    Task<IEnumerable<Exam>> GetOngoingExamsAsync();
    Task<bool> UpdateExamStatusAsync(Guid id, ExamStatus status);
    Task<int> GetExamAttemptCountAsync(Guid examId);
    Task<int> GetUserAttemptCountAsync(Guid examId, Guid userId);
    Task<bool> CanUserTakeExamAsync(Guid examId, Guid userId);
}
