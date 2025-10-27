using QuestionBank.Domain.Entities;
using QuestionBank.Domain.Enums;

namespace QuestionBank.Infrastructure.Repositories;

public interface IQuestionRepository : IRepository<Question>
{
    Task<Question?> GetQuestionWithKnowledgePointsAsync(Guid id);
    Task<Question?> GetQuestionWithSubQuestionsAsync(Guid id);
    Task<IEnumerable<Question>> GetQuestionsByTypeAsync(QuestionType type);
    Task<IEnumerable<Question>> GetQuestionsByDifficultyAsync(DifficultyLevel difficulty);
    Task<IEnumerable<Question>> GetQuestionsByChapterAsync(string chapter);
    Task<IEnumerable<Question>> GetQuestionsByCreatorAsync(Guid creatorId);
    Task<IEnumerable<Question>> GetEnabledQuestionsAsync();
    Task<IEnumerable<Question>> GetDisabledQuestionsAsync();
    Task<IEnumerable<Question>> GetQuestionsByKnowledgePointAsync(Guid knowledgePointId);
    Task<IEnumerable<Question>> GetMaterialQuestionsAsync();
    Task<IEnumerable<Question>> GetSubQuestionsByMaterialIdAsync(Guid materialQuestionId);
    Task<int> GetQuestionCountByTypeAsync(QuestionType type);
    Task<int> GetQuestionCountByDifficultyAsync(DifficultyLevel difficulty);
    Task<double> GetAverageScoreAsync();
    Task<IEnumerable<Question>> GetRandomQuestionsAsync(int count, QuestionType? type = null, DifficultyLevel? difficulty = null);
    Task<bool> UpdateQuestionStatusAsync(Guid id, QuestionStatus status);
    Task<Dictionary<QuestionType, int>> GetQuestionCountByTypeStatisticsAsync();
    Task<Dictionary<DifficultyLevel, int>> GetQuestionCountByDifficultyStatisticsAsync();
    Task<List<Question>> GetQuestionsByIdsAsync(List<Guid> questionIds);
}
