using Microsoft.EntityFrameworkCore;
using QuestionBank.Domain.Entities;
using QuestionBank.Domain.Enums;
using QuestionBank.Infrastructure.Data;

namespace QuestionBank.Infrastructure.Repositories;

public class QuestionRepository : Repository<Question>, IQuestionRepository
{
    public QuestionRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Question?> GetQuestionWithKnowledgePointsAsync(Guid id)
    {
        return await _dbSet
            .Include(q => q.QuestionKnowledgePoints)
                .ThenInclude(qkp => qkp.KnowledgePoint)
            .FirstOrDefaultAsync(q => q.Id == id);
    }

    public async Task<Question?> GetQuestionWithSubQuestionsAsync(Guid id)
    {
        return await _dbSet
            .Include(q => q.SubQuestions)
                .ThenInclude(sq => sq.QuestionKnowledgePoints)
                    .ThenInclude(qkp => qkp.KnowledgePoint)
            .FirstOrDefaultAsync(q => q.Id == id);
    }

    public async Task<IEnumerable<Question>> GetQuestionsByTypeAsync(QuestionType type)
    {
        return await _dbSet
            .Where(q => q.Type == type)
            .Include(q => q.QuestionKnowledgePoints)
                .ThenInclude(qkp => qkp.KnowledgePoint)
            .OrderByDescending(q => q.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Question>> GetQuestionsByDifficultyAsync(DifficultyLevel difficulty)
    {
        return await _dbSet
            .Where(q => q.Difficulty == difficulty)
            .Include(q => q.QuestionKnowledgePoints)
                .ThenInclude(qkp => qkp.KnowledgePoint)
            .OrderByDescending(q => q.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Question>> GetQuestionsByChapterAsync(string chapter)
    {
        return await _dbSet
            .Where(q => q.Chapter == chapter)
            .Include(q => q.QuestionKnowledgePoints)
                .ThenInclude(qkp => qkp.KnowledgePoint)
            .OrderBy(q => q.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Question>> GetQuestionsByCreatorAsync(Guid creatorId)
    {
        return await _dbSet
            .Where(q => q.CreatorId == creatorId)
            .Include(q => q.QuestionKnowledgePoints)
                .ThenInclude(qkp => qkp.KnowledgePoint)
            .OrderByDescending(q => q.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Question>> GetEnabledQuestionsAsync()
    {
        return await _dbSet
            .Where(q => q.Status == QuestionStatus.Enabled)
            .Include(q => q.QuestionKnowledgePoints)
                .ThenInclude(qkp => qkp.KnowledgePoint)
            .ToListAsync();
    }

    public async Task<IEnumerable<Question>> GetDisabledQuestionsAsync()
    {
        return await _dbSet
            .Where(q => q.Status == QuestionStatus.Disabled)
            .Include(q => q.QuestionKnowledgePoints)
                .ThenInclude(qkp => qkp.KnowledgePoint)
            .ToListAsync();
    }

    public async Task<IEnumerable<Question>> GetQuestionsByKnowledgePointAsync(Guid knowledgePointId)
    {
        return await _dbSet
            .Where(q => q.QuestionKnowledgePoints.Any(qkp => qkp.KnowledgePointId == knowledgePointId))
            .Include(q => q.QuestionKnowledgePoints)
                .ThenInclude(qkp => qkp.KnowledgePoint)
            .ToListAsync();
    }

    public async Task<IEnumerable<Question>> GetMaterialQuestionsAsync()
    {
        return await _dbSet
            .Where(q => q.Type == QuestionType.Material && q.MaterialQuestionId == null)
            .Include(q => q.SubQuestions)
            .ToListAsync();
    }

    public async Task<IEnumerable<Question>> GetSubQuestionsByMaterialIdAsync(Guid materialQuestionId)
    {
        return await _dbSet
            .Where(q => q.MaterialQuestionId == materialQuestionId)
            .OrderBy(q => q.SubQuestionOrder)
            .ToListAsync();
    }

    public async Task<int> GetQuestionCountByTypeAsync(QuestionType type)
    {
        return await _dbSet
            .CountAsync(q => q.Type == type);
    }

    public async Task<int> GetQuestionCountByDifficultyAsync(DifficultyLevel difficulty)
    {
        return await _dbSet
            .CountAsync(q => q.Difficulty == difficulty);
    }

    public async Task<double> GetAverageScoreAsync()
    {
        if (!await _dbSet.AnyAsync())
            return 0;

        return await _dbSet.AverageAsync(q => q.Score);
    }

    public async Task<IEnumerable<Question>> GetRandomQuestionsAsync(
        int count,
        QuestionType? type = null,
        DifficultyLevel? difficulty = null)
    {
        var query = _dbSet.AsQueryable();

        if (type.HasValue)
        {
            query = query.Where(q => q.Type == type.Value);
        }

        if (difficulty.HasValue)
        {
            query = query.Where(q => q.Difficulty == difficulty.Value);
        }

        query = query.Where(q => q.Status == QuestionStatus.Enabled);

        var questions = await query
            .Include(q => q.QuestionKnowledgePoints)
                .ThenInclude(qkp => qkp.KnowledgePoint)
            .ToListAsync();

        // 随机打乱并取指定数量
        var random = new Random();
        return questions.OrderBy(x => random.Next()).Take(count);
    }

    public async Task<bool> UpdateQuestionStatusAsync(Guid id, QuestionStatus status)
    {
        var question = await _dbSet.FindAsync(id);
        if (question == null)
            return false;

        question.Status = status;
        question.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<Dictionary<QuestionType, int>> GetQuestionCountByTypeStatisticsAsync()
    {
        var statistics = await _dbSet
            .GroupBy(q => q.Type)
            .Select(g => new { Type = g.Key, Count = g.Count() })
            .ToListAsync();

        return statistics.ToDictionary(x => x.Type, x => x.Count);
    }

    public async Task<Dictionary<DifficultyLevel, int>> GetQuestionCountByDifficultyStatisticsAsync()
    {
        var statistics = await _dbSet
            .GroupBy(q => q.Difficulty)
            .Select(g => new { Difficulty = g.Key, Count = g.Count() })
            .ToListAsync();

        return statistics.ToDictionary(x => x.Difficulty, x => x.Count);
    }

    public async Task<List<Question>> GetQuestionsByIdsAsync(List<Guid> questionIds)
    {
        return await _dbSet
            .Where(q => questionIds.Contains(q.Id))
            .Include(q => q.QuestionKnowledgePoints)
                .ThenInclude(qkp => qkp.KnowledgePoint)
            .ToListAsync();
    }
}
