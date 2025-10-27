# 题库管理系统 - 后台管理界面

基于 React + TypeScript + Material-UI + React Router v7 构建的现代化题库管理系统后台界面。

## 技术栈

- **前端框架**: React 18 + TypeScript
- **构建工具**: Vite 7
- **UI 组件库**: Material-UI (MUI) v6
- **路由管理**: React Router v7
- **HTTP 客户端**: Axios
- **状态管理**: React Context API
- **认证方式**: JWT Bearer Token

## 功能特性

### 管理员 (Admin)
- 完整的系统仪表板
- 题目管理（增删改查、高级筛选）
- 试卷管理
- 考试管理
- 知识点管理
- 用户管理
- 个人学习中心（收藏、笔记、错题本）

### 教师 (Teacher)
- 系统仪表板
- 题目管理
- 试卷管理
- 考试管理
- 知识点管理
- 个人学习中心

### 学生 (Student)
- 系统仪表板
- 个人学习中心（收藏、笔记、错题本）

## 快速开始

### 前置要求

- Node.js >= 18.0.0
- npm >= 9.0.0

### 安装依赖

```bash
npm install
```

### 配置环境变量

复制 `.env` 文件并根据需要修改：

```env
VITE_API_BASE_URL=http://localhost:5000/api
```

### 启动开发服务器

```bash
npm run dev
```

访问 http://localhost:5173

### 构建生产版本

```bash
npm run build
```

### 预览生产构建

```bash
npm run preview
```

## 项目结构

```
question-bank-admin/
├── src/
│   ├── api/                    # API 服务
│   │   └── apiService.ts       # 统一的 API 客户端
│   ├── components/             # 可复用组件
│   │   └── ProtectedRoute.tsx  # 路由保护组件
│   ├── contexts/               # React Context
│   │   └── AuthContext.tsx     # 认证上下文
│   ├── layouts/                # 布局组件
│   │   └── DashboardLayout.tsx # 主后台布局
│   ├── pages/                  # 页面组件
│   │   ├── LoginPage.tsx       # 登录/注册页
│   │   ├── DashboardPage.tsx   # 仪表板
│   │   └── QuestionsPage.tsx   # 题目管理
│   ├── types/                  # TypeScript 类型定义
│   │   └── index.ts            # 统一类型导出
│   ├── utils/                  # 工具函数
│   ├── App.tsx                 # 根组件
│   └── main.tsx                # 应用入口
├── .env                        # 环境变量
├── package.json
├── tsconfig.json
└── vite.config.ts
```

## 默认账户

开发环境可使用以下测试账户：

```
管理员:
用户名: admin
密码: admin123

教师:
用户名: teacher
密码: teacher123

学生:
用户名: student
密码: student123
```

## API 集成

### API 服务配置

API 服务位于 `src/api/apiService.ts`，包含：
- 自动 JWT Token 注入
- 401 错误自动登出
- 统一错误处理
- TypeScript 类型安全

### 主要 API 端点

#### 认证
- `POST /auth/login` - 用户登录
- `POST /auth/register` - 用户注册
- `GET /auth/profile` - 获取当前用户信息

#### 题目管理
- `POST /questions/search` - 搜索题目（分页、筛选）
- `GET /questions/:id` - 获取题目详情
- `POST /questions` - 创建题目
- `PUT /questions/:id` - 更新题目
- `DELETE /questions/:id` - 删除题目

#### 试卷管理
- `GET /papers` - 获取试卷列表
- `GET /papers/:id` - 获取试卷详情
- `POST /papers` - 创建试卷
- `PUT /papers/:id` - 更新试卷
- `DELETE /papers/:id` - 删除试卷

#### 学习功能
- `GET /learning/wrong-questions` - 获取错题
- `POST /learning/wrong-questions/:questionId` - 添加错题
- `GET /learning/favorites` - 获取收藏
- `POST /learning/favorites/:questionId` - 收藏题目
- `GET /learning/notes` - 获取笔记
- `POST /learning/notes` - 创建笔记

## 开发指南

### 代码规范

- 使用 TypeScript 严格模式
- 遵循 ESLint 配置
- 组件使用函数式组件 + Hooks
- 使用 React.FC 类型定义组件

### 添加新页面

1. 在 `src/pages/` 创建页面组件
2. 在 `src/types/index.ts` 添加必要类型
3. 在 `src/api/apiService.ts` 添加 API 方法
4. 在 `src/App.tsx` 添加路由配置
5. 在 `src/layouts/DashboardLayout.tsx` 添加菜单项（如需要）

### 路由保护

使用 `ProtectedRoute` 组件保护需要认证或特定角色的路由：

```tsx
<Route path="/users" element={
  <ProtectedRoute requiredRole={UserRole.Admin}>
    <UsersPage />
  </ProtectedRoute>
} />
```

### 状态管理

使用 `AuthContext` 管理用户认证状态：

```tsx
const { user, isAuthenticated, login, logout, hasRole } = useAuth();

// 检查角色
if (hasRole(UserRole.Teacher)) {
  // 教师专属功能
}
```

## 部署

### 部署到 Nginx

1. 构建项目：
```bash
npm run build
```

2. 将 `dist/` 目录内容复制到 Nginx 服务器

3. Nginx 配置示例：
```nginx
server {
    listen 80;
    server_name your-domain.com;
    root /var/www/question-bank-admin/dist;
    index index.html;

    location / {
        try_files $uri $uri/ /index.html;
    }

    # API 代理（可选）
    location /api {
        proxy_pass http://localhost:5000/api;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
    }
}
```

### 环境变量

生产环境需要修改 `.env` 中的 API 地址：

```env
VITE_API_BASE_URL=https://api.your-domain.com/api
```

## 常见问题

### 1. API 请求失败（401 错误）

检查：
- JWT Token 是否过期
- API Base URL 是否正确
- 后端 CORS 配置是否正确

### 2. 路由刷新后 404

确保服务器配置了 SPA 回退，所有路由都返回 `index.html`

### 3. 本地开发跨域问题

在 `vite.config.ts` 添加代理配置：

```ts
export default defineConfig({
  server: {
    proxy: {
      '/api': {
        target: 'http://localhost:5000',
        changeOrigin: true,
      }
    }
  }
})
```

## 待实现功能

- [ ] 试卷管理页面（PapersPage）
- [ ] 考试管理页面（ExamsPage）
- [ ] 知识点管理页面（KnowledgePointsPage）
- [ ] 用户管理页面（UsersPage）
- [ ] 我的收藏页面（FavoritesPage）
- [ ] 我的笔记页面（NotesPage）
- [ ] 错题本页面（WrongQuestionsPage）
- [ ] 设置页面（SettingsPage）
- [ ] 题目创建/编辑对话框
- [ ] 富文本编辑器集成
- [ ] 数据可视化图表
- [ ] 实时考试监控
- [ ] 单元测试

## 许可证

MIT

## 联系方式

如有问题或建议，请联系开发团队。
