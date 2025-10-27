using Microsoft.EntityFrameworkCore;
using QuestionBank.Domain.Entities;
using QuestionBank.Domain.Enums;
using QuestionBank.Infrastructure.Data;

namespace QuestionBank.Infrastructure.Repositories;

public class ExamRepository : Repository<Exam>, IExamRepository
{
    public ExamRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Exam?> GetExamWithPaperAsync(Guid id)
    {
        return await _dbSet
            .Include(e => e.Paper)
                .ThenInclude(p => p.PaperQuestions)
                    .ThenInclude(pq => pq.Question)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<Exam?> GetExamWithFullDetailsAsync(Guid id)
    {
        return await _dbSet
            .Include(e => e.Creator)
            .Include(e => e.Paper)
                .ThenInclude(p => p.PaperQuestions)
                    .ThenInclude(pq => pq.Question)
                        .ThenInclude(q => q.QuestionKnowledgePoints)
                            .ThenInclude(qkp => qkp.KnowledgePoint)
            .Include(e => e.ExamAttempts)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<IEnumerable<Exam>> GetExamsByStatusAsync(ExamStatus status)
    {
        return await _dbSet
            .Where(e => e.Status == status)
            .Include(e => e.Paper)
            .Include(e => e.Creator)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Exam>> GetExamsByCreatorAsync(Guid creatorId)
    {
        return await _dbSet
            .Where(e => e.CreatorId == creatorId)
            .Include(e => e.Paper)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Exam>> GetActiveExamsAsync()
    {
        var now = DateTime.UtcNow;
        return await _dbSet
            .Where(e => e.Status == ExamStatus.Published &&
                       (!e.StartTime.HasValue || e.StartTime.Value <= now) &&
                       (!e.EndTime.HasValue || e.EndTime.Value >= now))
            .Include(e => e.Paper)
            .Include(e => e.Creator)
            .OrderBy(e => e.StartTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<Exam>> GetUpcomingExamsAsync()
    {
        var now = DateTime.UtcNow;
        return await _dbSet
            .Where(e => e.Status == ExamStatus.Published &&
                       e.StartTime.HasValue &&
                       e.StartTime.Value > now)
            .Include(e => e.Paper)
            .Include(e => e.Creator)
            .OrderBy(e => e.StartTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<Exam>> GetOngoingExamsAsync()
    {
        var now = DateTime.UtcNow;
        return await _dbSet
            .Where(e => e.Status == ExamStatus.InProgress ||
                       (e.Status == ExamStatus.Published &&
                        e.StartTime.HasValue && e.StartTime.Value <= now &&
                        (!e.EndTime.HasValue || e.EndTime.Value >= now)))
            .Include(e => e.Paper)
            .Include(e => e.Creator)
            .ToListAsync();
    }

    public async Task<bool> UpdateExamStatusAsync(Guid id, ExamStatus status)
    {
        var exam = await _dbSet.FindAsync(id);
        if (exam == null)
            return false;

        exam.Status = status;
        exam.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<int> GetExamAttemptCountAsync(Guid examId)
    {
        return await _context.ExamAttempts
            .CountAsync(ea => ea.ExamId == examId);
    }

    public async Task<int> GetUserAttemptCountAsync(Guid examId, Guid userId)
    {
        return await _context.ExamAttempts
            .CountAsync(ea => ea.ExamId == examId && ea.UserId == userId);
    }

    public async Task<bool> CanUserTakeExamAsync(Guid examId, Guid userId)
    {
        var exam = await _dbSet.FindAsync(examId);
        if (exam == null)
            return false;

        // 检查考试状态
        if (exam.Status != ExamStatus.Published && exam.Status != ExamStatus.InProgress)
            return false;

        // 检查考试时间
        var now = DateTime.UtcNow;
        if (exam.StartTime.HasValue && exam.StartTime.Value > now)
            return false;

        if (exam.EndTime.HasValue && exam.EndTime.Value < now)
            return false;

        // 检查考试次数限制
        var attemptCount = await GetUserAttemptCountAsync(examId, userId);
        if (attemptCount >= exam.MaxAttempts)
            return false;

        return true;
    }
}
