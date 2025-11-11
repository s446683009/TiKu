using QuestionBank.Domain.Entities;

namespace QuestionBank.Domain.Repositories;

/// <summary>
/// 试卷仓储接口
/// </summary>
public interface IPaperRepository : IRepository<Paper>
{
    Task<Paper?> GetPaperWithQuestionsAsync(Guid id);
    Task<Paper?> GetPaperWithFullDetailsAsync(Guid id);
    Task<IEnumerable<Paper>> GetPapersByCreatorAsync(Guid creatorId);
    Task<IEnumerable<Paper>> GetRecentPapersAsync(int count = 10);
    Task<int> GetQuestionCountInPaperAsync(Guid paperId);
    Task<bool> AddQuestionsToPaperAsync(Guid paperId, List<PaperQuestion> paperQuestions);
    Task<bool> RemoveQuestionFromPaperAsync(Guid paperId, Guid questionId);
    Task<bool> UpdateQuestionOrderAsync(Guid paperId, Guid questionId, int newOrder);
    Task<bool> UpdateQuestionScoreAsync(Guid paperId, Guid questionId, int newScore);
    Task<Dictionary<Guid, int>> GetQuestionScoresAsync(Guid paperId);
}
