# 智能题库与在线评测平台 API

基于 ASP.NET Core 8.0 + PostgreSQL 的企业级题库管理系统

## 项目架构

本项目采用经典的分层架构(Clean Architecture):

```
QuestionBankAPI/
├── QuestionBank.Domain/          # 领域层 - 实体模型和枚举
│   ├── Entities/                 # 实体类
│   └── Enums/                    # 枚举类型
├── QuestionBank.Infrastructure/  # 基础设施层 - 数据访问
│   ├── Data/                     # DbContext
│   └── Repositories/             # 仓储模式实现
├── QuestionBank.Application/     # 应用层 - 业务逻辑
│   ├── DTOs/                     # 数据传输对象
│   └── Services/                 # 业务服务
└── QuestionBank.API/             # 表示层 - Web API
    └── Controllers/              # API控制器
```

## 核心功能

### 第一阶段 (MVP)
- ✅ 用户认证与授权 (JWT)
- ✅ 题库管理 (CRUD)
- ✅ 题目多条件搜索与分页
- ✅ 知识点体系管理
- ✅ 试卷管理 (手动组卷)
- ✅ 在线考试与答题
- ✅ 自动阅卷 (客观题)
- ✅ 错题本、收藏与笔记功能

### 第二阶段
- 智能自动组卷
- 学习数据分析
- 防作弊功能增强
- 题目批量导入导出

## 技术栈

- **框架**: ASP.NET Core 8.0
- **数据库**: PostgreSQL 16+
- **ORM**: Entity Framework Core 8.0
- **认证**: JWT Bearer Token
- **API文档**: Swagger/OpenAPI
- **密码加密**: BCrypt.Net

## 快速开始

### 前置要求

- .NET 8.0 SDK
- PostgreSQL 12+
- Visual Studio 2022 / VS Code / Rider

### 数据库配置

1. 安装 PostgreSQL 并创建数据库:
```sql
CREATE DATABASE questionbank;
```

2. 修改 `QuestionBank.API/appsettings.json` 中的连接字符串:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=questionbank;Username=postgres;Password=your_password"
  }
}
```

### 运行项目

1. 安装 EF Core 工具:
```bash
dotnet tool install --global dotnet-ef
```

2. 创建数据库迁移:
```bash
cd QuestionBank.Infrastructure
dotnet ef migrations add InitialCreate --startup-project ../QuestionBank.API
```

3. 更新数据库:
```bash
dotnet ef database update --startup-project ../QuestionBank.API
```

4. 运行项目:
```bash
cd ../QuestionBank.API
dotnet run
```

5. 访问 Swagger UI:
```
https://localhost:7000/swagger
```

## API 端点

### 认证模块
- `POST /api/auth/register` - 用户注册
- `POST /api/auth/login` - 用户登录

### 题目模块
- `GET /api/questions/{id}` - 获取题目详情
- `POST /api/questions/search` - 搜索题目(分页)
- `POST /api/questions` - 创建题目 (需要教师权限)
- `PUT /api/questions/{id}` - 更新题目 (需要教师权限)
- `DELETE /api/questions/{id}` - 删除题目 (需要教师权限)
- `GET /api/questions/by-knowledge-point/{id}` - 根据知识点获取题目

### 试卷模块
- `GET /api/papers/{id}` - 获取试卷基本信息
- `GET /api/papers/{id}/detail` - 获取试卷详细信息(含题目)
- `GET /api/papers` - 获取所有试卷(分页)
- `POST /api/papers` - 创建试卷 (需要教师权限)
- `PUT /api/papers/{id}` - 更新试卷 (需要教师权限)
- `DELETE /api/papers/{id}` - 删除试卷 (需要教师权限)

### 学习模块
#### 错题本
- `POST /api/learning/wrong-questions` - 添加题目到错题本
- `GET /api/learning/wrong-questions` - 获取我的错题本(分页)
- `DELETE /api/learning/wrong-questions/{questionId}` - 从错题本移除
- `GET /api/learning/wrong-questions/by-knowledge-point/{id}` - 按知识点获取错题

#### 收藏
- `POST /api/learning/favorites` - 收藏题目
- `GET /api/learning/favorites` - 获取我的收藏(分页)
- `DELETE /api/learning/favorites/{questionId}` - 取消收藏
- `PUT /api/learning/favorites/{questionId}/note` - 更新收藏备注

#### 笔记
- `POST /api/learning/notes` - 创建题目笔记
- `GET /api/learning/notes` - 获取我的所有笔记(分页)
- `GET /api/learning/notes/by-question/{questionId}` - 获取题目的笔记
- `PUT /api/learning/notes/{noteId}` - 更新笔记
- `DELETE /api/learning/notes/{noteId}` - 删除笔记

## 数据模型

### 核心实体

- **User** - 用户 (学员/教师/管理员)
- **Question** - 题目 (支持6种题型)
- **KnowledgePoint** - 知识点 (树形结构)
- **Paper** - 试卷
- **Exam** - 考试/练习
- **ExamAttempt** - 考试答题记录
- **Answer** - 答题详情
- **WrongQuestion** - 错题记录
- **FavoriteQuestion** - 收藏记录
- **QuestionNote** - 题目笔记

## 用户角色

- **Student** - 学员: 参加考试、练习、查看个人报告
- **Teacher** - 教师: 管理题库、组卷、批改试卷
- **Admin** - 管理员: 全部权限 + 用户管理

## 题目类型

1. **SingleChoice** - 单选题
2. **MultipleChoice** - 多选题
3. **TrueFalse** - 判断题
4. **FillBlank** - 填空题
5. **ShortAnswer** - 简答题
6. **Material** - 材料题 (一个材料下多个小题)

## 开发规范

### 命名规范
- 数据库表名: snake_case (如: `question_knowledge_points`)
- C# 类名: PascalCase (如: `QuestionService`)
- 方法名: PascalCase (如: `GetQuestionByIdAsync`)
- 变量名: camelCase (如: `userId`)

### 提交规范
- feat: 新功能
- fix: 修复bug
- docs: 文档更新
- refactor: 重构
- test: 测试相关

## 贡献指南

1. Fork 本项目
2. 创建特性分支 (`git checkout -b feature/AmazingFeature`)
3. 提交更改 (`git commit -m 'feat: Add some AmazingFeature'`)
4. 推送到分支 (`git push origin feature/AmazingFeature`)
5. 打开 Pull Request

## 许可证

本项目采用 MIT 许可证

## 联系方式

- 项目主页: [GitHub Repository]
- 问题反馈: [Issues]

## 更新日志

### v0.1.0 (2025-01-23)
- 初始化项目架构
- 实现用户认证模块
- 实现题库管理核心功能
- 添加 Swagger API 文档
