# 快速开始指南

## 1. 安装必要软件

### PostgreSQL
下载并安装 PostgreSQL 12 或更高版本:
- Windows: https://www.postgresql.org/download/windows/
- macOS: `brew install postgresql@16`
- Linux: `sudo apt-get install postgresql-16`

### .NET 8 SDK
下载并安装: https://dotnet.microsoft.com/download/dotnet/8.0

## 2. 配置数据库

### 创建数据库
```sql
-- 使用 psql 或 pgAdmin 连接到 PostgreSQL
CREATE DATABASE questionbank;
```

### 修改连接字符串
编辑 `QuestionBank.API/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=questionbank;Username=你的用户名;Password=你的密码"
  }
}
```

## 3. 运行数据库迁移

```bash
# 进入 Infrastructure 项目目录
cd QuestionBank.Infrastructure

# 安装 EF Core 工具(如果还没安装)
dotnet tool install --global dotnet-ef

# 创建初始迁移
dotnet ef migrations add InitialCreate --startup-project ../QuestionBank.API

# 更新数据库
dotnet ef database update --startup-project ../QuestionBank.API
```

## 4. 运行项目

```bash
# 进入 API 项目目录
cd ../QuestionBank.API

# 运行项目
dotnet run
```

项目将启动在:
- HTTP: http://localhost:5000
- HTTPS: https://localhost:7000

## 5. 访问 Swagger API 文档

在浏览器中打开:
```
https://localhost:7000/swagger
```

## 6. 测试 API

### 6.1 注册用户

**POST** `/api/auth/register`

```json
{
  "username": "testuser",
  "password": "Test123!",
  "email": "testuser@example.com",
  "fullName": "测试用户",
  "phone": "13800138000"
}
```

### 6.2 登录

**POST** `/api/auth/login`

```json
{
  "username": "testuser",
  "password": "Test123!"
}
```

响应中会返回 JWT Token:
```json
{
  "success": true,
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "user": {
      "id": "...",
      "username": "testuser",
      "email": "testuser@example.com",
      "fullName": "测试用户",
      "role": 1
    }
  }
}
```

### 6.3 使用Token访问受保护的API

在 Swagger UI 中:
1. 点击右上角的 "Authorize" 按钮
2. 输入: `Bearer 你的token` (注意Bearer后面有空格)
3. 点击 "Authorize"

现在可以调用需要认证的API了。

### 6.4 创建题目 (需要教师权限)

首先需要将用户角色更改为教师(Teacher):

```sql
-- 在 PostgreSQL 中执行
UPDATE users SET role = 2 WHERE username = 'testuser';
```

然后重新登录获取新的token,即可创建题目。

**POST** `/api/questions`

```json
{
  "type": 1,
  "content": "C#中,以下哪个关键字用于声明一个类?",
  "options": "[\"A. class\", \"B. struct\", \"C. interface\", \"D. enum\"]",
  "correctAnswer": "A",
  "explanation": "class关键字用于声明一个类",
  "difficulty": 1,
  "score": 2,
  "chapter": "第一章",
  "knowledgePointIds": []
}
```

### 6.5 搜索题目

**POST** `/api/questions/search`

```json
{
  "keyword": "C#",
  "type": 1,
  "pageNumber": 1,
  "pageSize": 10
}
```

## 7. 初始化测试数据(可选)

如果想快速测试,可以运行初始化脚本:

```bash
# 在 PostgreSQL 中执行
psql -U postgres -d questionbank -f database-init.sql
```

这会创建:
- 管理员账户: admin / Admin123!
- 教师账户: teacher1 / Teacher123!
- 学生账户: student1 / Student123!
- 示例知识点和题目

## 8. 常见问题

### 连接数据库失败
- 检查 PostgreSQL 服务是否运行
- 检查用户名和密码是否正确
- 检查数据库名是否存在
- 检查防火墙设置

### 端口被占用
修改 `QuestionBank.API/Properties/launchSettings.json` 中的端口号

### JWT Token 无效
- 检查 token 是否过期(默认7天)
- 检查 appsettings.json 中的 JWT 配置是否一致
- 重新登录获取新的 token

## 9. 开发工具推荐

- **IDE**: Visual Studio 2022 / JetBrains Rider / VS Code
- **数据库管理**: pgAdmin 4 / DBeaver
- **API测试**: Postman / Swagger UI
- **Git客户端**: GitHub Desktop / SourceTree

## 10. 下一步

- 查看 [README.md](README.md) 了解项目架构
- 查看 Swagger 文档了解所有 API 端点
- 开始开发新功能或修改现有功能
- 运行单元测试(待添加)

## 技术支持

遇到问题? 请查看:
- 项目 Issues: [GitHub Issues]
- 文档: [Wiki]
- 讨论区: [Discussions]
