namespace QuestionBank.Domain.Repositories;

/// <summary>
/// 工作单元接口 - 管理事务和仓储生命周期
/// </summary>
public interface IUnitOfWork : IDisposable
{
    IRepository<T> Repository<T>() where T : class;
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
