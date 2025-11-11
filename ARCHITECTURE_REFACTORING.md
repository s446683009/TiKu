# Clean Architecture 重构指南 - 仓储模式正确实现

## 🎯 问题分析

### 当前架构问题

```
Application Layer (应用层)
    ↓ depends on
Infrastructure Layer (基础设施层)
    ├── IRepository (接口) ❌ 接口不应该在这里
    └── Repository (实现)  ✅ 实现应该在这里
```

**问题**：
1. **违反依赖倒置原则**：高层模块（Application）依赖低层模块（Infrastructure）
2. **难以更换实现**：接口和实现在同一层，无法解耦
3. **测试困难**：无法轻松 Mock 仓储接口
4. **违反 Clean Architecture**：依赖方向错误

## ✅ 正确的架构设计

### 依赖关系

```
┌────────────────────────────────────────┐
│           API Layer                    │
│       (Controllers)                    │
└──────────────┬─────────────────────────┘
               │ depends on
┌──────────────▼─────────────────────────┐
│      Application Layer                 │
│  (Services, Use Cases, DTOs)           │
└──────────────┬─────────────────────────┘
               │ depends on
┌──────────────▼─────────────────────────┐
│         Domain Layer                   │  ← 核心层，不依赖任何层
│    (Entities, Interfaces)              │
│  ┌─────────────────────────────────┐   │
│  │   Repositories/                 │   │
│  │   ├── IRepository<T>            │   │
│  │   ├── IUserRepository           │   │
│  │   ├── IQuestionRepository       │   │
│  │   └── IUnitOfWork               │   │
│  └─────────────────────────────────┘   │
└────────────────────────────────────────┘
               ↑ implements (实现接口)
┌──────────────┴─────────────────────────┐
│     Infrastructure Layer               │  ← 最外层，依赖 Domain
│  (Data Access, External Services)      │
│  ┌─────────────────────────────────┐   │
│  │   Repositories/                 │   │
│  │   ├── Repository<T>             │   │
│  │   ├── UserRepository            │   │
│  │   ├── QuestionRepository        │   │
│  │   └── UnitOfWork                │   │
│  └─────────────────────────────────┘   │
│  ├── ApplicationDbContext              │
│  └── Configurations/                   │
└────────────────────────────────────────┘
```

### 依赖流向

```
API → Application → Domain ← Infrastructure
```

**关键原则**：
- Domain 层是核心，不依赖任何其他层
- Infrastructure 层依赖 Domain 层（实现接口）
- Application 层依赖 Domain 层（使用接口）
- API 层依赖 Application 和 Domain 层

## 📁 正确的目录结构

```
QuestionBank.Domain/
├── Entities/
│   ├── User.cs
│   ├── Question.cs
│   └── ...
├── Enums/
│   └── UserRole.cs
├── Repositories/           ← 仓储接口定义在这里
│   ├── IRepository.cs
│   ├── IUserRepository.cs
│   ├── IQuestionRepository.cs
│   ├── IPaperRepository.cs
│   ├── IExamRepository.cs
│   └── IUnitOfWork.cs
└── ValueObjects/
    └── ...

QuestionBank.Infrastructure/
├── Data/
│   ├── ApplicationDbContext.cs
│   └── Configurations/
├── Repositories/           ← 仓储实现在这里
│   ├── Repository.cs       (实现 IRepository<T>)
│   ├── UserRepository.cs   (实现 IUserRepository)
│   ├── QuestionRepository.cs
│   ├── PaperRepository.cs
│   ├── ExamRepository.cs
│   └── UnitOfWork.cs       (实现 IUnitOfWork)
└── ...
```

## 🔨 重构步骤

### 步骤 1: 将接口移到 Domain 层

#### 创建 Domain/Repositories/IRepository.cs

```csharp
using System.Linq.Expressions;

namespace QuestionBank.Domain.Repositories;

/// <summary>
/// 通用仓储接口 - 定义在 Domain 层
/// </summary>
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task<T> AddAsync(T entity);
    Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);
}
```

#### 创建 Domain/Repositories/IUserRepository.cs

```csharp
using QuestionBank.Domain.Entities;
using QuestionBank.Domain.Enums;

namespace QuestionBank.Domain.Repositories;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByEmailAsync(string email);
    Task<bool> UsernameExistsAsync(string username);
    Task<bool> EmailExistsAsync(string email);
    Task<IEnumerable<User>> GetUsersByRoleAsync(UserRole role);
}
```

#### 创建 Domain/Repositories/IUnitOfWork.cs

```csharp
namespace QuestionBank.Domain.Repositories;

public interface IUnitOfWork : IDisposable
{
    IRepository<T> Repository<T>() where T : class;
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
```

### 步骤 2: 更新 Infrastructure 层实现

#### Infrastructure/Repositories/Repository.cs

```csharp
using Microsoft.EntityFrameworkCore;
using QuestionBank.Domain.Repositories;  // ← 引用 Domain 层接口
using QuestionBank.Infrastructure.Data;
using System.Linq.Expressions;

namespace QuestionBank.Infrastructure.Repositories;

/// <summary>
/// 通用仓储实现 - EF Core 实现
/// </summary>
public class Repository<T> : IRepository<T> where T : class
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(Guid id)
    {
        return await _dbSet.FindAsync(id);
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    // ... 其他实现
}
```

### 步骤 3: 更新依赖注入配置

#### API/Program.cs

```csharp
// ✅ 正确：注册接口（Domain）到实现（Infrastructure）
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();
// ...
```

## 🔄 如何更换仓储实现

现在架构正确后，更换实现变得非常简单！

### 场景 1: 从 EF Core 切换到 Dapper

#### 1. 创建新的实现（不影响现有代码）

```csharp
// Infrastructure/Repositories/Dapper/DapperUserRepository.cs
using Dapper;
using QuestionBank.Domain.Repositories;  // 使用相同的接口
using QuestionBank.Domain.Entities;

namespace QuestionBank.Infrastructure.Repositories.Dapper;

public class DapperUserRepository : IUserRepository
{
    private readonly IDbConnection _connection;

    public DapperUserRepository(IDbConnection connection)
    {
        _connection = connection;
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        const string sql = "SELECT * FROM Users WHERE Id = @Id";
        return await _connection.QueryFirstOrDefaultAsync<User>(sql, new { Id = id });
    }

    // 实现其他接口方法...
}
```

#### 2. 修改依赖注入（唯一需要修改的地方）

```csharp
// Program.cs
// 从 EF Core 实现切换到 Dapper 实现
// builder.Services.AddScoped<IUserRepository, UserRepository>();  // ❌ EF Core
builder.Services.AddScoped<IUserRepository, DapperUserRepository>();  // ✅ Dapper
```

### 场景 2: 使用 MongoDB

```csharp
// Infrastructure/Repositories/MongoDB/MongoUserRepository.cs
using MongoDB.Driver;
using QuestionBank.Domain.Repositories;
using QuestionBank.Domain.Entities;

namespace QuestionBank.Infrastructure.Repositories.MongoDB;

public class MongoUserRepository : IUserRepository
{
    private readonly IMongoCollection<User> _users;

    public MongoUserRepository(IMongoDatabase database)
    {
        _users = database.GetCollection<User>("users");
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await _users.Find(u => u.Id == id).FirstOrDefaultAsync();
    }

    // 实现其他接口方法...
}
```

### 场景 3: 使用缓存层（装饰器模式）

```csharp
// Infrastructure/Repositories/Cached/CachedUserRepository.cs
using Microsoft.Extensions.Caching.Memory;
using QuestionBank.Domain.Repositories;
using QuestionBank.Domain.Entities;

namespace QuestionBank.Infrastructure.Repositories.Cached;

/// <summary>
/// 缓存装饰器 - 在原有实现上添加缓存层
/// </summary>
public class CachedUserRepository : IUserRepository
{
    private readonly IUserRepository _innerRepository;
    private readonly IMemoryCache _cache;

    public CachedUserRepository(
        IUserRepository innerRepository,
        IMemoryCache cache)
    {
        _innerRepository = innerRepository;
        _cache = cache;
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        string cacheKey = $"user_{id}";

        if (_cache.TryGetValue(cacheKey, out User? cachedUser))
        {
            return cachedUser;
        }

        var user = await _innerRepository.GetByIdAsync(id);

        if (user != null)
        {
            _cache.Set(cacheKey, user, TimeSpan.FromMinutes(10));
        }

        return user;
    }

    // 委托其他方法到内部仓储...
}
```

#### 注册缓存装饰器

```csharp
// Program.cs
builder.Services.AddMemoryCache();

// 注册原始实现
builder.Services.AddScoped<UserRepository>();

// 用装饰器包装
builder.Services.AddScoped<IUserRepository>(sp =>
{
    var innerRepo = sp.GetRequiredService<UserRepository>();
    var cache = sp.GetRequiredService<IMemoryCache>();
    return new CachedUserRepository(innerRepo, cache);
});
```

## 🧪 单元测试优势

正确的架构使得单元测试变得简单：

```csharp
// Tests/Application/UserServiceTests.cs
using Moq;
using QuestionBank.Domain.Repositories;
using QuestionBank.Application.Services;

public class UserServiceTests
{
    [Fact]
    public async Task GetUserById_ShouldReturnUser_WhenUserExists()
    {
        // Arrange
        var mockRepo = new Mock<IUserRepository>();
        mockRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new User { Id = Guid.NewGuid(), Username = "test" });

        var service = new UserService(mockRepo.Object);

        // Act
        var result = await service.GetUserByIdAsync(Guid.NewGuid());

        // Assert
        Assert.NotNull(result);
        mockRepo.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Once);
    }
}
```

## 📊 架构对比

| 方面 | ❌ 错误架构 | ✅ 正确架构 |
|------|------------|------------|
| 接口位置 | Infrastructure | Domain |
| 依赖方向 | Application → Infrastructure | Application → Domain ← Infrastructure |
| 更换实现 | 困难，需修改多处 | 简单，只改 DI 配置 |
| 单元测试 | 困难 | 容易，轻松 Mock |
| 符合原则 | 违反 DIP | 符合 DIP、OCP |

## 🎯 总结

### 关键要点

1. **接口归属**：仓储接口属于领域概念，应该在 Domain 层
2. **依赖倒置**：Infrastructure 依赖 Domain，而不是相反
3. **易于扩展**：新增实现只需要实现接口，修改 DI 配置
4. **测试友好**：可以轻松 Mock 接口进行单元测试

### 实践建议

1. ✅ Domain 层定义业务规则和接口
2. ✅ Infrastructure 层实现技术细节
3. ✅ Application 层通过接口使用仓储
4. ✅ API 层只负责 HTTP 处理
5. ✅ 使用依赖注入管理生命周期

这就是 Clean Architecture 的核心思想：**让依赖指向核心（Domain），而不是指向细节（Infrastructure）**！
