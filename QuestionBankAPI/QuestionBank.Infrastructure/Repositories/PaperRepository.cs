using Microsoft.EntityFrameworkCore;
using QuestionBank.Domain.Entities;
using QuestionBank.Infrastructure.Data;

namespace QuestionBank.Infrastructure.Repositories;

public class PaperRepository : Repository<Paper>, IPaperRepository
{
    public PaperRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Paper?> GetPaperWithQuestionsAsync(Guid id)
    {
        return await _dbSet
            .Include(p => p.PaperQuestions)
                .ThenInclude(pq => pq.Question)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Paper?> GetPaperWithFullDetailsAsync(Guid id)
    {
        return await _dbSet
            .Include(p => p.Creator)
            .Include(p => p.PaperQuestions)
                .ThenInclude(pq => pq.Question)
                    .ThenInclude(q => q.QuestionKnowledgePoints)
                        .ThenInclude(qkp => qkp.KnowledgePoint)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IEnumerable<Paper>> GetPapersByCreatorAsync(Guid creatorId)
    {
        return await _dbSet
            .Where(p => p.CreatorId == creatorId)
            .Include(p => p.PaperQuestions)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Paper>> GetRecentPapersAsync(int count = 10)
    {
        return await _dbSet
            .Include(p => p.Creator)
            .Include(p => p.PaperQuestions)
            .OrderByDescending(p => p.CreatedAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task<int> GetQuestionCountInPaperAsync(Guid paperId)
    {
        return await _context.PaperQuestions
            .CountAsync(pq => pq.PaperId == paperId);
    }

    public async Task<bool> AddQuestionsToPaperAsync(Guid paperId, List<PaperQuestion> paperQuestions)
    {
        var paper = await _dbSet.FindAsync(paperId);
        if (paper == null)
            return false;

        await _context.PaperQuestions.AddRangeAsync(paperQuestions);

        // 更新试卷总分
        paper.TotalScore = await _context.PaperQuestions
            .Where(pq => pq.PaperId == paperId)
            .SumAsync(pq => pq.Score);

        paper.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveQuestionFromPaperAsync(Guid paperId, Guid questionId)
    {
        var paperQuestion = await _context.PaperQuestions
            .FirstOrDefaultAsync(pq => pq.PaperId == paperId && pq.QuestionId == questionId);

        if (paperQuestion == null)
            return false;

        _context.PaperQuestions.Remove(paperQuestion);

        var paper = await _dbSet.FindAsync(paperId);
        if (paper != null)
        {
            paper.TotalScore = await _context.PaperQuestions
                .Where(pq => pq.PaperId == paperId)
                .SumAsync(pq => pq.Score);
            paper.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateQuestionOrderAsync(Guid paperId, Guid questionId, int newOrder)
    {
        var paperQuestion = await _context.PaperQuestions
            .FirstOrDefaultAsync(pq => pq.PaperId == paperId && pq.QuestionId == questionId);

        if (paperQuestion == null)
            return false;

        paperQuestion.QuestionOrder = newOrder;

        var paper = await _dbSet.FindAsync(paperId);
        if (paper != null)
        {
            paper.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateQuestionScoreAsync(Guid paperId, Guid questionId, int newScore)
    {
        var paperQuestion = await _context.PaperQuestions
            .FirstOrDefaultAsync(pq => pq.PaperId == paperId && pq.QuestionId == questionId);

        if (paperQuestion == null)
            return false;

        paperQuestion.Score = newScore;

        var paper = await _dbSet.FindAsync(paperId);
        if (paper != null)
        {
            paper.TotalScore = await _context.PaperQuestions
                .Where(pq => pq.PaperId == paperId)
                .SumAsync(pq => pq.Score);
            paper.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<Dictionary<Guid, int>> GetQuestionScoresAsync(Guid paperId)
    {
        var scores = await _context.PaperQuestions
            .Where(pq => pq.PaperId == paperId)
            .Select(pq => new { pq.QuestionId, pq.Score })
            .ToListAsync();

        return scores.ToDictionary(x => x.QuestionId, x => x.Score);
    }
}
