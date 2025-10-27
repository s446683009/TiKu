using Microsoft.EntityFrameworkCore;
using QuestionBank.Domain.Entities;
using QuestionBank.Domain.Enums;
using QuestionBank.Infrastructure.Data;

namespace QuestionBank.Infrastructure.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _dbSet
            .FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _dbSet
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<bool> UsernameExistsAsync(string username)
    {
        return await _dbSet
            .AnyAsync(u => u.Username == username);
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _dbSet
            .AnyAsync(u => u.Email == email);
    }

    public async Task<IEnumerable<User>> GetUsersByRoleAsync(UserRole role)
    {
        return await _dbSet
            .Where(u => u.Role == role)
            .OrderBy(u => u.FullName)
            .ToListAsync();
    }

    public async Task<IEnumerable<User>> GetActiveUsersAsync()
    {
        return await _dbSet
            .Where(u => u.IsActive)
            .OrderBy(u => u.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<User>> GetInactiveUsersAsync()
    {
        return await _dbSet
            .Where(u => !u.IsActive)
            .OrderBy(u => u.CreatedAt)
            .ToListAsync();
    }

    public async Task<User?> GetUserWithExamAttemptsAsync(Guid userId)
    {
        return await _dbSet
            .Include(u => u.ExamAttempts)
                .ThenInclude(ea => ea.Exam)
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task<User?> GetUserWithWrongQuestionsAsync(Guid userId)
    {
        return await _dbSet
            .Include(u => u.WrongQuestions)
                .ThenInclude(wq => wq.Question)
                    .ThenInclude(q => q.QuestionKnowledgePoints)
                        .ThenInclude(qkp => qkp.KnowledgePoint)
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task<User?> GetUserWithFavoritesAsync(Guid userId)
    {
        return await _dbSet
            .Include(u => u.FavoriteQuestions)
                .ThenInclude(fq => fq.Question)
                    .ThenInclude(q => q.QuestionKnowledgePoints)
                        .ThenInclude(qkp => qkp.KnowledgePoint)
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task<int> GetTotalUserCountAsync()
    {
        return await _dbSet.CountAsync();
    }

    public async Task<int> GetUserCountByRoleAsync(UserRole role)
    {
        return await _dbSet
            .CountAsync(u => u.Role == role);
    }

    public async Task<bool> UpdateLastLoginAsync(Guid userId)
    {
        var user = await _dbSet.FindAsync(userId);
        if (user == null)
            return false;

        user.LastLoginAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ActivateUserAsync(Guid userId)
    {
        var user = await _dbSet.FindAsync(userId);
        if (user == null)
            return false;

        user.IsActive = true;
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeactivateUserAsync(Guid userId)
    {
        var user = await _dbSet.FindAsync(userId);
        if (user == null)
            return false;

        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }
}
