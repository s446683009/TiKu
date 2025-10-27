# 题库管理系统

一个完整的在线题库管理系统，支持题目、试卷、考试管理以及学生学习功能。

## 🎯 项目特性

### 核心功能

- **用户认证**: JWT 令牌认证，基于角色的权限控制
- **题目管理**: 支持6种题型（单选、多选、判断、填空、简答、材料题）
- **试卷管理**: 组卷功能，支持多题目关联
- **考试管理**: 完整的考试创建、监控、统计功能
- **知识点管理**: 树形结构的知识点体系
- **学习功能**: 错题本、题目收藏、学习笔记
- **用户管理**: 管理员、教师、学生三级权限体系

### 技术亮点

- **后端**: ASP.NET Core 8.0 + Clean Architecture + PostgreSQL
- **前端**: React 18 + TypeScript + Material-UI + Vite
- **容器化**: Docker + Docker Compose 一键部署
- **安全性**: BCrypt 密码加密、JWT 认证、HTTPS 支持
- **可扩展**: 微服务架构，支持水平扩展

## 📸 系统截图

（此处可添加截图）

## 🚀 快速开始

### 方式一：使用 Docker（推荐）

```bash
# 1. 克隆项目
git clone <repository-url>
cd TiKu

# 2. 配置环境变量
cp .env.example .env
# 编辑 .env 文件，修改数据库密码和 JWT 密钥

# 3. 一键启动
./start.sh dev

# 4. 访问应用
# 前端: http://localhost
# 后端: http://localhost:5000
```

### 方式二：本地开发

#### 后端

```bash
cd QuestionBankAPI

# 安装依赖（已包含在项目中）
dotnet restore

# 配置数据库连接字符串
# 编辑 QuestionBank.API/appsettings.Development.json

# 运行迁移
dotnet ef database update --project QuestionBank.Infrastructure

# 启动应用
dotnet run --project QuestionBank.API
```

#### 前端

```bash
cd question-bank-admin

# 安装依赖
npm install

# 配置 API 地址
# 编辑 .env 文件

# 启动开发服务器
npm run dev
```

## 📚 文档

- [Docker 部署指南](DOCKER_DEPLOYMENT.md) - Docker 容器化部署完整文档
- [后端 API 文档](QuestionBankAPI/README.md) - 后端 API 详细说明
- [前端文档](question-bank-admin/README.md) - 前端应用说明

## 🏗️ 项目结构

```
TiKu/
├── QuestionBankAPI/              # 后端 API
│   ├── QuestionBank.API/         # Web API 层
│   ├── QuestionBank.Application/ # 应用业务逻辑层
│   ├── QuestionBank.Domain/      # 领域模型层
│   ├── QuestionBank.Infrastructure/ # 基础设施层
│   └── Dockerfile                # 后端 Docker 配置
├── question-bank-admin/          # 前端应用
│   ├── src/
│   │   ├── api/                  # API 服务
│   │   ├── components/           # 可复用组件
│   │   ├── contexts/             # React Context
│   │   ├── layouts/              # 布局组件
│   │   ├── pages/                # 页面组件
│   │   └── types/                # TypeScript 类型
│   ├── Dockerfile                # 前端 Docker 配置
│   └── nginx.conf                # Nginx 配置
├── nginx/                        # 生产环境 Nginx 配置
├── docker-compose.yml            # 开发环境编排
├── docker-compose.prod.yml       # 生产环境编排
├── start.sh                      # 快速启动脚本
└── .env.example                  # 环境变量模板
```

## 🔧 技术栈

### 后端

- **框架**: ASP.NET Core 8.0 Web API
- **数据库**: PostgreSQL 16
- **ORM**: Entity Framework Core 8.0
- **认证**: JWT Bearer Token
- **架构**: Clean Architecture (洋葱架构)
- **依赖注入**: Microsoft.Extensions.DependencyInjection

### 前端

- **框架**: React 18 + TypeScript
- **构建工具**: Vite 7
- **UI 组件**: Material-UI (MUI) v7
- **路由**: React Router v7
- **HTTP 客户端**: Axios
- **状态管理**: React Context API

### DevOps

- **容器化**: Docker + Docker Compose
- **Web 服务器**: Nginx (生产环境)
- **反向代理**: Nginx
- **SSL/TLS**: Let's Encrypt

## 👥 用户角色

### 管理员 (Admin)
- 所有系统功能
- 用户管理
- 系统配置

### 教师 (Teacher)
- 题目管理
- 试卷管理
- 考试管理
- 知识点管理
- 学习功能

### 学生 (Student)
- 查看题目
- 参加考试
- 学习功能（错题本、收藏、笔记）

## 🔒 安全性

- ✅ BCrypt 密码加密
- ✅ JWT 令牌认证
- ✅ 基于角色的访问控制 (RBAC)
- ✅ HTTPS 支持
- ✅ SQL 注入防护 (EF Core)
- ✅ XSS 防护
- ✅ CORS 配置

## 🎬 默认账户

开发环境测试账户：

| 角色 | 用户名 | 密码 |
|------|--------|------|
| 管理员 | admin | admin123 |
| 教师 | teacher | teacher123 |
| 学生 | student | student123 |

⚠️ **生产环境必须修改默认密码！**

## 📊 API 端点

### 认证
- `POST /api/auth/login` - 用户登录
- `POST /api/auth/register` - 用户注册
- `GET /api/users/me` - 获取当前用户信息

### 题目管理
- `POST /api/questions/search` - 搜索题目（分页）
- `GET /api/questions/:id` - 获取题目详情
- `POST /api/questions` - 创建题目
- `PUT /api/questions/:id` - 更新题目
- `DELETE /api/questions/:id` - 删除题目

### 试卷管理
- `GET /api/papers` - 获取试卷列表
- `GET /api/papers/:id/detail` - 获取试卷详情
- `POST /api/papers` - 创建试卷
- `PUT /api/papers/:id` - 更新试卷
- `DELETE /api/papers/:id` - 删除试卷

### 学习功能
- `GET /api/learning/wrong-questions` - 错题本
- `POST /api/learning/wrong-questions/:questionId` - 添加错题
- `GET /api/learning/favorites` - 收藏列表
- `POST /api/learning/favorites/:questionId` - 收藏题目
- `GET /api/learning/notes` - 笔记列表
- `POST /api/learning/notes` - 创建笔记

更多 API 详见 Swagger 文档：`http://localhost:5000/swagger`

## 🔄 开发流程

```bash
# 1. 创建功能分支
git checkout -b feature/new-feature

# 2. 开发并测试
# 后端
cd QuestionBankAPI && dotnet test

# 前端
cd question-bank-admin && npm run build

# 3. 提交代码
git add .
git commit -m "feat: add new feature"

# 4. 推送并创建 PR
git push origin feature/new-feature
```

## 🐛 故障排查

### 后端无法启动
```bash
# 检查数据库连接
docker-compose logs postgres

# 重置数据库
docker-compose down -v
docker-compose up -d
```

### 前端无法访问 API
检查 `.env` 文件中的 `VITE_API_BASE_URL` 配置

### Docker 容器无法启动
```bash
# 查看日志
docker-compose logs

# 重新构建
docker-compose build --no-cache
```

更多问题请查看 [Docker 部署指南](DOCKER_DEPLOYMENT.md)

## 📈 性能优化

- ✅ 数据库索引优化
- ✅ EF Core 查询优化
- ✅ 前端代码分割
- ✅ Nginx Gzip 压缩
- ✅ 静态资源缓存
- ✅ 连接池配置

## 🚧 未来计划

- [ ] 题目导入/导出（Excel、Word）
- [ ] 在线考试实时监控
- [ ] 数据统计和图表分析
- [ ] 移动端适配
- [ ] 微信小程序
- [ ] AI 智能组卷
- [ ] 多租户支持

## 🤝 贡献

欢迎贡献代码！请遵循以下步骤：

1. Fork 本仓库
2. 创建特性分支 (`git checkout -b feature/amazing-feature`)
3. 提交更改 (`git commit -m 'Add amazing feature'`)
4. 推送到分支 (`git push origin feature/amazing-feature`)
5. 创建 Pull Request

## 📄 许可证

本项目采用 MIT 许可证 - 详见 [LICENSE](LICENSE) 文件

## 👨‍💻 作者

您的名字

## 🙏 致谢

- [ASP.NET Core](https://dotnet.microsoft.com/apps/aspnet)
- [React](https://react.dev/)
- [Material-UI](https://mui.com/)
- [PostgreSQL](https://www.postgresql.org/)
- [Docker](https://www.docker.com/)

## 📞 联系方式

- Email: your-email@example.com
- Issue: <repository-url>/issues

---

⭐ 如果这个项目对你有帮助，请给个 Star！
