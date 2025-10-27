# 题库系统API完整文档

## 项目完成度

### ✅ 已实现功能

1. **用户认证与授权**
   - 用户注册/登录
   - JWT Token认证
   - 基于角色的权限控制(学员/教师/管理员)
   - BCrypt密码加密

2. **题库管理**
   - 题目CRUD操作
   - 多条件搜索和分页
   - 6种题型支持
   - 知识点管理(树形结构)
   - 题目难度等级(1-5星)

3. **用户管理**
   - 用户信息管理
   - 修改密码
   - 用户激活/禁用
   - 用户统计数据
   - 系统统计数据

4. **考试系统**
   - 考试创建和管理
   - 开始考试
   - 在线答题
   - 提交考试
   - 自动批改客观题
   - 考试记录查询

5. **知识点管理**
   - 树形结构知识点
   - 多级分类
   - 知识点CRUD

6. **试卷管理**
   - 试卷CRUD操作
   - 手动组卷
   - 试卷详情查看（含题目列表）
   - 分页查询
   - 删除保护（防止删除已用于考试的试卷）

7. **学习功能** ✅ 新增
   - **错题本**: 错题记录、查询、移除、按知识点筛选
   - **收藏**: 题目收藏、取消收藏、备注管理
   - **笔记**: 题目笔记创建、查询、更新、删除

8. **基础设施**
   - 全局异常处理
   - 分层架构设计
   - Repository模式
   - Unit of Work模式

## API端点总览

### 认证模块 (AuthController)
| 方法 | 端点 | 说明 | 权限 |
|------|------|------|------|
| POST | `/api/auth/register` | 用户注册 | 公开 |
| POST | `/api/auth/login` | 用户登录 | 公开 |

### 用户管理 (UsersController)
| 方法 | 端点 | 说明 | 权限 |
|------|------|------|------|
| GET | `/api/users/me` | 获取当前用户信息 | 已登录 |
| GET | `/api/users/{id}` | 获取用户详情 | 管理员 |
| GET | `/api/users` | 获取所有用户(分页) | 管理员 |
| GET | `/api/users/by-role/{role}` | 按角色获取用户 | 管理员 |
| PUT | `/api/users/{id}` | 更新用户信息 | 本人/管理员 |
| POST | `/api/users/{id}/change-password` | 修改密码 | 本人 |
| POST | `/api/users/{id}/activate` | 激活用户 | 管理员 |
| POST | `/api/users/{id}/deactivate` | 禁用用户 | 管理员 |
| GET | `/api/users/{id}/stats` | 获取用户统计 | 本人/教师/管理员 |
| GET | `/api/users/system/stats` | 获取系统统计 | 管理员 |

### 题目管理 (QuestionsController)
| 方法 | 端点 | 说明 | 权限 |
|------|------|------|------|
| GET | `/api/questions/{id}` | 获取题目详情 | 已登录 |
| POST | `/api/questions/search` | 搜索题目(分页) | 已登录 |
| POST | `/api/questions` | 创建题目 | 教师/管理员 |
| PUT | `/api/questions/{id}` | 更新题目 | 教师/管理员 |
| DELETE | `/api/questions/{id}` | 删除题目 | 教师/管理员 |
| GET | `/api/questions/by-knowledge-point/{id}` | 根据知识点获取题目 | 已登录 |

### 试卷管理 (PapersController)
| 方法 | 端点 | 说明 | 权限 |
|------|------|------|------|
| GET | `/api/papers/{id}` | 获取试卷基本信息 | 已登录 |
| GET | `/api/papers/{id}/detail` | 获取试卷详细信息(含题目) | 已登录 |
| GET | `/api/papers` | 获取所有试卷(分页) | 已登录 |
| POST | `/api/papers` | 创建试卷(手动组卷) | 教师/管理员 |
| PUT | `/api/papers/{id}` | 更新试卷 | 教师/管理员 |
| DELETE | `/api/papers/{id}` | 删除试卷 | 教师/管理员 |

### 学习功能 (LearningController) ✅ 新增
| 方法 | 端点 | 说明 | 权限 |
|------|------|------|------|
| POST | `/api/learning/wrong-questions` | 添加题目到错题本 | 已登录 |
| GET | `/api/learning/wrong-questions` | 获取我的错题本(分页) | 已登录 |
| DELETE | `/api/learning/wrong-questions/{questionId}` | 从错题本移除 | 已登录 |
| GET | `/api/learning/wrong-questions/by-knowledge-point/{knowledgePointId}` | 按知识点获取错题 | 已登录 |
| POST | `/api/learning/favorites` | 收藏题目 | 已登录 |
| GET | `/api/learning/favorites` | 获取我的收藏(分页) | 已登录 |
| DELETE | `/api/learning/favorites/{questionId}` | 取消收藏 | 已登录 |
| PUT | `/api/learning/favorites/{questionId}/note` | 更新收藏备注 | 已登录 |
| POST | `/api/learning/notes` | 创建题目笔记 | 已登录 |
| GET | `/api/learning/notes` | 获取我的所有笔记(分页) | 已登录 |
| GET | `/api/learning/notes/by-question/{questionId}` | 获取题目的笔记 | 已登录 |
| PUT | `/api/learning/notes/{noteId}` | 更新笔记 | 已登录 |
| DELETE | `/api/learning/notes/{noteId}` | 删除笔记 | 已登录 |

### 考试管理 (ExamsController)
| 方法 | 端点 | 说明 | 权限 |
|------|------|------|------|
| GET | `/api/exams/{id}` | 获取考试详情 | 已登录 |
| GET | `/api/exams` | 获取所有考试 | 已登录 |
| POST | `/api/exams` | 创建考试 | 教师/管理员 |
| POST | `/api/exams/{examId}/start` | 开始考试 | 已登录 |
| POST | `/api/exams/submit-answer` | 提交单题答案 | 已登录 |
| POST | `/api/exams/attempts/{examAttemptId}/submit` | 提交整个考试 | 已登录 |
| GET | `/api/exams/my-attempts` | 获取我的考试记录 | 已登录 |

### 知识点管理 (KnowledgePointsController)
| 方法 | 端点 | 说明 | 权限 |
|------|------|------|------|
| GET | `/api/knowledgepoints/{id}` | 获取知识点详情 | 已登录 |
| GET | `/api/knowledgepoints` | 获取所有知识点 | 已登录 |
| GET | `/api/knowledgepoints/roots` | 获取根级知识点 | 已登录 |
| GET | `/api/knowledgepoints/{parentId}/children` | 获取子知识点 | 已登录 |
| GET | `/api/knowledgepoints/{id}/tree` | 获取知识点树 | 已登录 |
| POST | `/api/knowledgepoints` | 创建知识点 | 教师/管理员 |
| PUT | `/api/knowledgepoints/{id}` | 更新知识点 | 教师/管理员 |
| DELETE | `/api/knowledgepoints/{id}` | 删除知识点 | 管理员 |

## 使用流程示例

### 1. 注册和登录

```http
### 注册学生账户
POST http://localhost:5000/api/auth/register
Content-Type: application/json

{
  "username": "student1",
  "password": "Student123!",
  "email": "student1@example.com",
  "fullName": "张三",
  "phone": "13800138000"
}

### 登录获取Token
POST http://localhost:5000/api/auth/login
Content-Type: application/json

{
  "username": "student1",
  "password": "Student123!"
}

### 响应示例
{
  "success": true,
  "message": "登录成功",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "user": {
      "id": "xxx-xxx-xxx",
      "username": "student1",
      "email": "student1@example.com",
      "fullName": "张三",
      "role": 1,
      "isActive": true
    }
  }
}
```

### 2. 创建知识点(需要教师权限)

```http
### 创建一级知识点
POST http://localhost:5000/api/knowledgepoints
Authorization: Bearer {token}
Content-Type: application/json

{
  "name": "C#编程",
  "description": "C#编程语言基础"
}

### 创建二级知识点
POST http://localhost:5000/api/knowledgepoints
Authorization: Bearer {token}
Content-Type: application/json

{
  "name": "面向对象",
  "description": "C#面向对象编程",
  "parentId": "{一级知识点ID}"
}
```

### 3. 创建题目(需要教师权限)

```http
### 创建单选题
POST http://localhost:5000/api/questions
Authorization: Bearer {token}
Content-Type: application/json

{
  "type": 1,
  "content": "C#中,以下哪个关键字用于声明一个类?",
  "options": "[\"A. class\", \"B. struct\", \"C. interface\", \"D. enum\"]",
  "correctAnswer": "A",
  "explanation": "class关键字用于声明一个类",
  "difficulty": 1,
  "score": 2,
  "chapter": "第一章",
  "knowledgePointIds": ["{知识点ID}"]
}

### 创建多选题
POST http://localhost:5000/api/questions
Authorization: Bearer {token}
Content-Type: application/json

{
  "type": 2,
  "content": "C#中,以下哪些是值类型?",
  "options": "[\"A. int\", \"B. string\", \"C. double\", \"D. bool\"]",
  "correctAnswer": "A,C,D",
  "explanation": "int、double和bool是值类型,string是引用类型",
  "difficulty": 3,
  "score": 5,
  "chapter": "第一章",
  "knowledgePointIds": []
}
```

### 4. 搜索题目

```http
### 多条件搜索题目
POST http://localhost:5000/api/questions/search
Authorization: Bearer {token}
Content-Type: application/json

{
  "keyword": "C#",
  "type": 1,
  "difficulty": 1,
  "pageNumber": 1,
  "pageSize": 10
}
```

### 5. 创建试卷（手动组卷，需要教师权限）

```http
### 创建试卷
POST http://localhost:5000/api/papers
Authorization: Bearer {token}
Content-Type: application/json

{
  "title": "C#基础知识测试",
  "description": "涵盖C#基础语法和面向对象编程",
  "duration": 60,
  "questions": [
    {
      "questionId": "题目ID-1",
      "questionOrder": 1,
      "score": 5
    },
    {
      "questionId": "题目ID-2",
      "questionOrder": 2,
      "score": 10
    },
    {
      "questionId": "题目ID-3",
      "questionOrder": 3,
      "score": 5
    }
  ]
}

### 响应示例
{
  "success": true,
  "message": "试卷创建成功",
  "data": {
    "id": "xxx-xxx-xxx",
    "title": "C#基础知识测试",
    "description": "涵盖C#基础语法和面向对象编程",
    "totalScore": 20,
    "duration": 60,
    "questionCount": 3,
    "createdAt": "2025-01-23T10:00:00Z"
  }
}

### 获取试卷详情（包含题目列表）
GET http://localhost:5000/api/papers/{paperId}/detail
Authorization: Bearer {token}

### 获取所有试卷（分页）
GET http://localhost:5000/api/papers?pageNumber=1&pageSize=20
Authorization: Bearer {token}

### 更新试卷
PUT http://localhost:5000/api/papers/{paperId}
Authorization: Bearer {token}
Content-Type: application/json

{
  "title": "C#基础知识测试（更新版）",
  "description": "涵盖C#基础语法和面向对象编程",
  "duration": 90,
  "questions": [
    {
      "questionId": "题目ID-1",
      "questionOrder": 1,
      "score": 10
    },
    {
      "questionId": "题目ID-2",
      "questionOrder": 2,
      "score": 10
    }
  ]
}

### 删除试卷
DELETE http://localhost:5000/api/papers/{paperId}
Authorization: Bearer {token}
```

### 6. 创建和参加考试

```http
### 创建考试(需要教师权限)
POST http://localhost:5000/api/exams
Authorization: Bearer {token}
Content-Type: application/json

{
  "title": "C#基础测试",
  "description": "测试C#基础知识",
  "paperId": "{试卷ID}",
  "startTime": "2025-01-25T09:00:00Z",
  "endTime": "2025-01-25T11:00:00Z",
  "duration": 60,
  "maxAttempts": 2,
  "answerDisplayMode": 1,
  "allowPause": false,
  "shuffleQuestions": true,
  "shuffleOptions": true,
  "requireFullScreen": false,
  "disableCopyPaste": true
}

### 开始考试
POST http://localhost:5000/api/exams/{examId}/start
Authorization: Bearer {token}

### 提交单题答案
POST http://localhost:5000/api/exams/submit-answer
Authorization: Bearer {token}
Content-Type: application/json

{
  "examAttemptId": "{答题记录ID}",
  "questionId": "{题目ID}",
  "userAnswer": "A"
}

### 提交整个考试
POST http://localhost:5000/api/exams/attempts/{examAttemptId}/submit
Authorization: Bearer {token}

### 查看我的考试记录
GET http://localhost:5000/api/exams/my-attempts
Authorization: Bearer {token}
```

### 7. 学习功能使用（错题本、收藏、笔记）

```http
### 添加题目到错题本
POST http://localhost:5000/api/learning/wrong-questions
Authorization: Bearer {token}
Content-Type: application/json

{
  "questionId": "{题目ID}"
}

### 获取我的错题本(分页)
GET http://localhost:5000/api/learning/wrong-questions?pageNumber=1&pageSize=20
Authorization: Bearer {token}

### 按知识点获取错题
GET http://localhost:5000/api/learning/wrong-questions/by-knowledge-point/{knowledgePointId}
Authorization: Bearer {token}

### 从错题本移除题目
DELETE http://localhost:5000/api/learning/wrong-questions/{questionId}
Authorization: Bearer {token}

### 收藏题目
POST http://localhost:5000/api/learning/favorites
Authorization: Bearer {token}
Content-Type: application/json

{
  "questionId": "{题目ID}",
  "note": "这道题很重要，需要重点复习"
}

### 获取我的收藏列表(分页)
GET http://localhost:5000/api/learning/favorites?pageNumber=1&pageSize=20
Authorization: Bearer {token}

### 更新收藏备注
PUT http://localhost:5000/api/learning/favorites/{questionId}/note
Authorization: Bearer {token}
Content-Type: application/json

{
  "note": "更新后的备注内容"
}

### 取消收藏
DELETE http://localhost:5000/api/learning/favorites/{questionId}
Authorization: Bearer {token}

### 创建题目笔记
POST http://localhost:5000/api/learning/notes
Authorization: Bearer {token}
Content-Type: application/json

{
  "questionId": "{题目ID}",
  "content": "这道题的关键是理解面向对象的概念..."
}

### 获取题目的笔记
GET http://localhost:5000/api/learning/notes/by-question/{questionId}
Authorization: Bearer {token}

### 获取我的所有笔记(分页)
GET http://localhost:5000/api/learning/notes?pageNumber=1&pageSize=20
Authorization: Bearer {token}

### 更新笔记
PUT http://localhost:5000/api/learning/notes/{noteId}
Authorization: Bearer {token}
Content-Type: application/json

{
  "content": "更新后的笔记内容..."
}

### 删除笔记
DELETE http://localhost:5000/api/learning/notes/{noteId}
Authorization: Bearer {token}
```

### 8. 查看统计数据

```http
### 查看个人统计
GET http://localhost:5000/api/users/{userId}/stats
Authorization: Bearer {token}

### 查看系统统计(管理员)
GET http://localhost:5000/api/users/system/stats
Authorization: Bearer {token}
```

## 数据模型

### 题目类型枚举
- 1: SingleChoice (单选题)
- 2: MultipleChoice (多选题)
- 3: TrueFalse (判断题)
- 4: FillBlank (填空题)
- 5: ShortAnswer (简答题)
- 6: Material (材料题)

### 难度等级枚举
- 1: VeryEasy (非常简单)
- 2: Easy (简单)
- 3: Medium (中等)
- 4: Hard (困难)
- 5: VeryHard (非常困难)

### 用户角色枚举
- 1: Student (学员)
- 2: Teacher (教师)
- 3: Admin (管理员)

### 考试状态枚举
- 0: Draft (草稿)
- 1: Published (已发布)
- 2: InProgress (进行中)
- 3: Ended (已结束)
- 4: Cancelled (已取消)

## 错误处理

所有API返回统一格式:

### 成功响应
```json
{
  "success": true,
  "message": "操作成功",
  "data": { ... }
}
```

### 错误响应
```json
{
  "success": false,
  "message": "错误信息",
  "errors": ["详细错误1", "详细错误2"]
}
```

### HTTP状态码
- 200: 成功
- 201: 创建成功
- 400: 请求错误
- 401: 未授权
- 403: 禁止访问
- 404: 资源不存在
- 500: 服务器错误

## 分页响应格式

```json
{
  "success": true,
  "data": {
    "items": [...],
    "totalCount": 100,
    "pageNumber": 1,
    "pageSize": 20,
    "totalPages": 5,
    "hasPrevious": false,
    "hasNext": true
  }
}
```

## 安全性

1. **密码安全**: 使用BCrypt加密存储
2. **JWT Token**: 7天有效期
3. **角色权限**: 基于角色的访问控制
4. **CORS**: 已配置跨域支持
5. **异常处理**: 全局异常处理中间件

## 性能优化

1. **Include优化**: 避免N+1查询
2. **分页查询**: 支持大数据量
3. **索引优化**: 关键字段建立索引
4. **缓存**: 可扩展缓存支持

## 未来扩展

- ✅ ~~试卷管理和组卷功能~~ (已完成)
- ✅ ~~错题本完整实现~~ (已完成)
- ✅ ~~收藏功能~~ (已完成)
- ✅ ~~笔记功能~~ (已完成)
- 学习数据分析
- 题目批量导入导出
- 实时监控
- 防作弊功能增强
- 智能自动组卷
