# User Repository 使用文档

## 概述

`UserRepository` 是专门为 `User` 实体创建的仓储类,提供了比通用 `Repository<T>` 更丰富的用户相关查询方法。

## 架构

```
IUserRepository (接口)
    ↓ 继承
IRepository<User> (通用仓储接口)
    ↓ 实现
UserRepository (实现类)
    ↓ 继承
Repository<User> (通用仓储实现)
```

## 接口定义

### 基础查询

#### GetByUsernameAsync
根据用户名获取用户
```csharp
Task<User?> GetByUsernameAsync(string username);
```

**示例:**
```csharp
var user = await _userRepository.GetByUsernameAsync("admin");
if (user != null)
{
    Console.WriteLine($"找到用户: {user.FullName}");
}
```

#### GetByEmailAsync
根据邮箱获取用户
```csharp
Task<User?> GetByEmailAsync(string email);
```

**示例:**
```csharp
var user = await _userRepository.GetByEmailAsync("admin@example.com");
```

#### UsernameExistsAsync
检查用户名是否已存在
```csharp
Task<bool> UsernameExistsAsync(string username);
```

**示例:**
```csharp
if (await _userRepository.UsernameExistsAsync("newuser"))
{
    return "用户名已存在";
}
```

#### EmailExistsAsync
检查邮箱是否已被注册
```csharp
Task<bool> EmailExistsAsync(string email);
```

### 角色和状态查询

#### GetUsersByRoleAsync
获取指定角色的所有用户
```csharp
Task<IEnumerable<User>> GetUsersByRoleAsync(UserRole role);
```

**示例:**
```csharp
// 获取所有教师
var teachers = await _userRepository.GetUsersByRoleAsync(UserRole.Teacher);

// 获取所有学生
var students = await _userRepository.GetUsersByRoleAsync(UserRole.Student);
```

#### GetActiveUsersAsync
获取所有活跃用户
```csharp
Task<IEnumerable<User>> GetActiveUsersAsync();
```

#### GetInactiveUsersAsync
获取所有被禁用的用户
```csharp
Task<IEnumerable<User>> GetInactiveUsersAsync();
```

### 关联查询

#### GetUserWithExamAttemptsAsync
获取用户及其考试记录
```csharp
Task<User?> GetUserWithExamAttemptsAsync(Guid userId);
```

**说明:** 包含 `ExamAttempts` 和 `Exam` 的关联数据

**示例:**
```csharp
var user = await _userRepository.GetUserWithExamAttemptsAsync(userId);
if (user != null)
{
    foreach (var attempt in user.ExamAttempts)
    {
        Console.WriteLine($"考试: {attempt.Exam.Title}, 分数: {attempt.TotalScore}");
    }
}
```

#### GetUserWithWrongQuestionsAsync
获取用户及其错题本
```csharp
Task<User?> GetUserWithWrongQuestionsAsync(Guid userId);
```

**说明:** 包含 `WrongQuestions`、`Question` 和 `KnowledgePoints` 的关联数据

**示例:**
```csharp
var user = await _userRepository.GetUserWithWrongQuestionsAsync(userId);
if (user != null)
{
    var activeWrongQuestions = user.WrongQuestions.Where(wq => !wq.IsRemoved);
    Console.WriteLine($"错题数量: {activeWrongQuestions.Count()}");
}
```

#### GetUserWithFavoritesAsync
获取用户及其收藏题目
```csharp
Task<User?> GetUserWithFavoritesAsync(Guid userId);
```

**说明:** 包含 `FavoriteQuestions`、`Question` 和 `KnowledgePoints` 的关联数据

### 统计查询

#### GetTotalUserCountAsync
获取用户总数
```csharp
Task<int> GetTotalUserCountAsync();
```

#### GetUserCountByRoleAsync
获取指定角色的用户数量
```csharp
Task<int> GetUserCountByRoleAsync(UserRole role);
```

**示例:**
```csharp
var studentCount = await _userRepository.GetUserCountByRoleAsync(UserRole.Student);
var teacherCount = await _userRepository.GetUserCountByRoleAsync(UserRole.Teacher);
var adminCount = await _userRepository.GetUserCountByRoleAsync(UserRole.Admin);

Console.WriteLine($"学生: {studentCount}, 教师: {teacherCount}, 管理员: {adminCount}");
```

### 状态更新

#### UpdateLastLoginAsync
更新用户最后登录时间
```csharp
Task<bool> UpdateLastLoginAsync(Guid userId);
```

**示例:**
```csharp
await _userRepository.UpdateLastLoginAsync(userId);
```

#### ActivateUserAsync
激活用户
```csharp
Task<bool> ActivateUserAsync(Guid userId);
```

**示例:**
```csharp
if (await _userRepository.ActivateUserAsync(userId))
{
    return "用户已激活";
}
```

#### DeactivateUserAsync
禁用用户
```csharp
Task<bool> DeactivateUserAsync(Guid userId);
```

## 依赖注入配置

在 `Program.cs` 中注册:

```csharp
builder.Services.AddScoped<IUserRepository, UserRepository>();
```

## 使用示例

### 在 Service 中使用

```csharp
public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserDto?> GetUserByUsernameAsync(string username)
    {
        var user = await _userRepository.GetByUsernameAsync(username);
        return user != null ? MapToDto(user) : null;
    }

    public async Task<bool> IsUsernameAvailableAsync(string username)
    {
        return !await _userRepository.UsernameExistsAsync(username);
    }
}
```

### 在 Controller 中直接使用 (不推荐)

```csharp
[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly IUserRepository _userRepository;

    public AdminController(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    [HttpGet("teachers")]
    public async Task<IActionResult> GetAllTeachers()
    {
        var teachers = await _userRepository.GetUsersByRoleAsync(UserRole.Teacher);
        return Ok(teachers);
    }
}
```

## 性能优化建议

### 1. 使用 Include 避免 N+1 查询

```csharp
// ✅ 好的做法 - 一次查询获取所有数据
var user = await _userRepository.GetUserWithExamAttemptsAsync(userId);

// ❌ 不好的做法 - 会产生多次查询
var user = await _userRepository.GetByIdAsync(userId);
var attempts = await _context.ExamAttempts.Where(e => e.UserId == userId).ToListAsync();
```

### 2. 只查询需要的字段

```csharp
// 如果只需要统计数量,使用 Count 方法
var count = await _userRepository.GetTotalUserCountAsync();

// 而不是
var users = await _userRepository.GetAllAsync();
var count = users.Count();
```

### 3. 合理使用缓存

对于不常变化的数据,可以考虑添加缓存:

```csharp
public class CachedUserRepository : IUserRepository
{
    private readonly IUserRepository _innerRepository;
    private readonly IMemoryCache _cache;

    public async Task<int> GetTotalUserCountAsync()
    {
        return await _cache.GetOrCreateAsync("user_count", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
            return await _innerRepository.GetTotalUserCountAsync();
        });
    }
}
```

## 扩展 UserRepository

如果需要添加新的查询方法:

### 1. 在接口中添加方法签名

```csharp
public interface IUserRepository : IRepository<User>
{
    // ... 现有方法

    // 新增方法
    Task<IEnumerable<User>> GetRecentlyRegisteredUsersAsync(int days);
}
```

### 2. 在实现类中实现方法

```csharp
public class UserRepository : Repository<User>, IUserRepository
{
    // ... 现有实现

    public async Task<IEnumerable<User>> GetRecentlyRegisteredUsersAsync(int days)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-days);
        return await _dbSet
            .Where(u => u.CreatedAt >= cutoffDate)
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync();
    }
}
```

## 单元测试示例

```csharp
public class UserRepositoryTests
{
    private DbContextOptions<ApplicationDbContext> CreateNewContextOptions()
    {
        return new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public async Task GetByUsernameAsync_ExistingUser_ReturnsUser()
    {
        // Arrange
        var options = CreateNewContextOptions();
        using (var context = new ApplicationDbContext(options))
        {
            var user = new User
            {
                Username = "testuser",
                Email = "test@example.com",
                PasswordHash = "hash",
                FullName = "Test User"
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();
        }

        // Act
        using (var context = new ApplicationDbContext(options))
        {
            var repository = new UserRepository(context);
            var result = await repository.GetByUsernameAsync("testuser");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("testuser", result.Username);
        }
    }
}
```

## 常见问题

### Q: UserRepository 和通用 Repository 有什么区别?

A: `UserRepository` 继承自 `Repository<User>`,除了拥有通用仓储的所有方法外,还提供了用户特定的查询方法,如根据用户名查询、角色筛选等。

### Q: 什么时候使用 UserRepository,什么时候使用 IRepository<User>?

A:
- 使用 `IUserRepository`: 需要用户特定查询时(如按用户名、邮箱查询)
- 使用 `IRepository<User>`: 只需要基本CRUD操作时

### Q: 如何在 UnitOfWork 中使用 UserRepository?

A: 可以扩展 UnitOfWork:

```csharp
public interface IUnitOfWork : IDisposable
{
    IRepository<T> Repository<T>() where T : class;
    IUserRepository Users { get; }
    Task<int> SaveChangesAsync();
}

public class UnitOfWork : IUnitOfWork
{
    private IUserRepository? _userRepository;

    public IUserRepository Users =>
        _userRepository ??= new UserRepository(_context);
}
```

## 相关文档

- [Repository Pattern 详解](./docs/repository-pattern.md)
- [Unit of Work Pattern](./docs/unit-of-work.md)
- [Entity Framework Core 最佳实践](./docs/ef-core-best-practices.md)
