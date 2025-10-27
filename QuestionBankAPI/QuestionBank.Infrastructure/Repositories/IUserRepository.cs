using QuestionBank.Domain.Entities;
using QuestionBank.Domain.Enums;

namespace QuestionBank.Infrastructure.Repositories;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByEmailAsync(string email);
    Task<bool> UsernameExistsAsync(string username);
    Task<bool> EmailExistsAsync(string email);
    Task<IEnumerable<User>> GetUsersByRoleAsync(UserRole role);
    Task<IEnumerable<User>> GetActiveUsersAsync();
    Task<IEnumerable<User>> GetInactiveUsersAsync();
    Task<User?> GetUserWithExamAttemptsAsync(Guid userId);
    Task<User?> GetUserWithWrongQuestionsAsync(Guid userId);
    Task<User?> GetUserWithFavoritesAsync(Guid userId);
    Task<int> GetTotalUserCountAsync();
    Task<int> GetUserCountByRoleAsync(UserRole role);
    Task<bool> UpdateLastLoginAsync(Guid userId);
    Task<bool> ActivateUserAsync(Guid userId);
    Task<bool> DeactivateUserAsync(Guid userId);
}
