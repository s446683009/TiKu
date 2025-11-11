# 🚀 仓储模式快速重构指南

## 当前状态

✅ 已创建 Domain 层仓储接口：
- `QuestionBank.Domain/Repositories/IRepository.cs`
- `QuestionBank.Domain/Repositories/IUnitOfWork.cs`
- `QuestionBank.Domain/Repositories/IUserRepository.cs`
- `QuestionBank.Domain/Repositories/IQuestionRepository.cs`
- `QuestionBank.Domain/Repositories/IPaperRepository.cs`
- `QuestionBank.Domain/Repositories/IExamRepository.cs`

## 📝 迁移步骤

### 步骤 1: 更新 Infrastructure 层的仓储实现

修改所有仓储实现文件的命名空间引用：

```csharp
// 修改前
using QuestionBank.Infrastructure.Repositories;  // ❌

// 修改后
using QuestionBank.Domain.Repositories;  // ✅
```

### 步骤 2: 更新具体文件

#### UserRepository.cs

```bash
# 打开文件
QuestionBankAPI/QuestionBank.Infrastructure/Repositories/UserRepository.cs
```

**修改这一行**：
```csharp
// 第 1-3 行，修改 using 语句
using QuestionBank.Domain.Entities;
using QuestionBank.Domain.Enums;
using QuestionBank.Domain.Repositories;  // ← 修改这里
using QuestionBank.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
```

对以下文件重复相同操作：
- `QuestionRepository.cs`
- `PaperRepository.cs`
- `ExamRepository.cs`
- `Repository.cs`
- `UnitOfWork.cs`

### 步骤 3: 更新 Application 层服务

Application 层服务现在引用 Domain 层接口：

```csharp
// QuestionBank.Application/Services/UserService.cs
using QuestionBank.Domain.Repositories;  // ✅ 引用 Domain 层接口

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }
}
```

### 步骤 4: 删除旧的接口文件（可选）

```bash
# 删除 Infrastructure 层中的接口文件
cd QuestionBankAPI/QuestionBank.Infrastructure/Repositories

rm IRepository.cs
rm IUnitOfWork.cs
rm IUserRepository.cs
rm IQuestionRepository.cs
rm IPaperRepository.cs
rm IExamRepository.cs
```

### 步骤 5: 验证编译

```bash
cd QuestionBankAPI
dotnet build

# 如果有编译错误，检查命名空间引用
```

## 🎯 快速查找替换

使用编辑器的查找替换功能：

**查找**: `using QuestionBank.Infrastructure.Repositories;`
**替换**: `using QuestionBank.Domain.Repositories;`

**适用文件**:
- `QuestionBank.Infrastructure/Repositories/*.cs`（实现文件）
- `QuestionBank.Application/Services/*.cs`（服务文件）

## ✅ 验证清单

- [ ] Domain 层有所有接口定义
- [ ] Infrastructure 层实现引用 Domain 接口
- [ ] Application 层服务引用 Domain 接口
- [ ] Program.cs 依赖注入配置正确
- [ ] 编译无错误
- [ ] 单元测试通过

## 📊 架构验证

正确的依赖关系应该是：

```
API ─depends on─> Application ─depends on─> Domain
                                              ↑
                                              │
                                         implements
                                              │
                                       Infrastructure
```

使用此命令验证依赖：
```bash
dotnet list QuestionBank.Application package
# 应该只看到 Domain 层，不应该看到 Infrastructure

dotnet list QuestionBank.Infrastructure package
# 应该看到 Domain 层
```

## 🔄 现在可以轻松更换实现！

### 示例：切换到 Dapper

1. 创建 Dapper 实现：
```csharp
// QuestionBank.Infrastructure/Repositories/Dapper/DapperUserRepository.cs
public class DapperUserRepository : IUserRepository { ... }
```

2. 修改 Program.cs：
```csharp
// 只需修改这一行！
builder.Services.AddScoped<IUserRepository, DapperUserRepository>();
```

### 示例：添加缓存层

```csharp
// 装饰器模式
builder.Services.Decorate<IUserRepository, CachedUserRepository>();
```

## 🎉 完成！

现在你的架构符合 Clean Architecture 原则，享受：
- ✅ 依赖倒置
- ✅ 易于测试
- ✅ 灵活切换实现
- ✅ 解耦合

详细文档请参考 [ARCHITECTURE_REFACTORING.md](ARCHITECTURE_REFACTORING.md)
