# Docker 部署指南

题库管理系统的 Docker 容器化部署文档。

## 📋 目录

- [前置要求](#前置要求)
- [快速开始](#快速开始)
- [开发环境部署](#开发环境部署)
- [生产环境部署](#生产环境部署)
- [配置说明](#配置说明)
- [常用命令](#常用命令)
- [故障排查](#故障排查)

## 前置要求

- Docker >= 20.10
- Docker Compose >= 2.0
- 至少 4GB 可用内存
- 至少 10GB 可用磁盘空间

### 检查 Docker 版本

```bash
docker --version
docker-compose --version
```

## 快速开始

### 1. 克隆项目

```bash
git clone <repository-url>
cd TiKu
```

### 2. 配置环境变量

```bash
# 复���环境变量模板
cp .env.example .env

# 编辑环境变量（重要！）
nano .env
```

**必须修改的配置**：
- `POSTGRES_PASSWORD`: 数据库密码
- `JWT_SECRET_KEY`: JWT 密钥（至少32个字符）

### 3. 启动所有服务

```bash
# 构建并启动
docker-compose up -d

# 查看日志
docker-compose logs -f
```

### 4. 访问应用

- **前端**: http://localhost
- **后端 API**: http://localhost:5000
- **数据库**: localhost:5432

### 5. 初始化数据库

首次启动后，后端会自动创建数据库表结构。

## 开发环境部署

### 使用 docker-compose.yml

```bash
# 启动所有服务
docker-compose up -d

# 查看服务状态
docker-compose ps

# 查看日志
docker-compose logs -f backend
docker-compose logs -f frontend

# 重启某个服务
docker-compose restart backend

# 停止所有服务
docker-compose down
```

### 开发模式的热重载

如果需要代码热重载，建议本地开发：

**后端**:
```bash
cd QuestionBankAPI
dotnet run --project QuestionBank.API
```

**前端**:
```bash
cd question-bank-admin
npm run dev
```

## 生产环境部署

### 1. 使用生产配置

```bash
# 使用生产环境配置文件
docker-compose -f docker-compose.prod.yml up -d
```

### 2. 配置环境变量

```bash
# 编辑生产环境变量
nano .env
```

**生产环境必须配置**:
```env
POSTGRES_PASSWORD=<强密码>
JWT_SECRET_KEY=<至少32字符的随机字符串>
VITE_API_BASE_URL=https://api.yourdomain.com/api
```

### 3. 配置 HTTPS（推荐）

#### 使用 Let's Encrypt

```bash
# 安装 certbot
sudo apt-get install certbot

# 获取证书
sudo certbot certonly --standalone -d yourdomain.com -d www.yourdomain.com

# 复制证书到项目目录
sudo cp /etc/letsencrypt/live/yourdomain.com/fullchain.pem nginx/ssl/
sudo cp /etc/letsencrypt/live/yourdomain.com/privkey.pem nginx/ssl/
```

#### 启用 HTTPS 配置

编辑 `nginx/conf.d/default.conf`，取消 HTTPS 部分的注释。

### 4. 启动生产服务

```bash
docker-compose -f docker-compose.prod.yml up -d
```

### 5. 设置自动重启

服务已配置 `restart: always`，系统重启后会自动启动。

## 配置说明

### 容器端口映射

| 服务 | 容器端口 | 主机端口 | 说明 |
|------|---------|---------|------|
| Frontend | 80 | 80 | 前端应用 |
| Backend | 5000 | 5000 | 后端 API |
| PostgreSQL | 5432 | 5432 | 数据库 |
| Nginx (生产) | 80/443 | 80/443 | 反向代理 |

### 数据持久化

使用 Docker volumes 持久化数据：

- `postgres_data`: 数据库数据
- `./logs`: 应用日志
- `./backups`: 数据库备份

### 资源限制（生产环境）

在 `docker-compose.prod.yml` 中配置了资源限制：

```yaml
deploy:
  resources:
    limits:
      cpus: '2'
      memory: 2G
    reservations:
      cpus: '0.5'
      memory: 512M
```

## 常用命令

### 容器管理

```bash
# 查看运行的容器
docker-compose ps

# 启动服务
docker-compose start

# 停止服务
docker-compose stop

# 重启服务
docker-compose restart

# 删除容器（保留数据）
docker-compose down

# 删除容器和数据卷
docker-compose down -v
```

### 日志查看

```bash
# 查看所有日志
docker-compose logs

# 实时跟踪日志
docker-compose logs -f

# 查看特定服务日志
docker-compose logs backend
docker-compose logs frontend
docker-compose logs postgres

# 查看最近100行日志
docker-compose logs --tail=100
```

### 进入容器

```bash
# 进入后端容器
docker-compose exec backend sh

# 进入数据库容器
docker-compose exec postgres psql -U postgres -d QuestionBankDB

# 进入前端容器
docker-compose exec frontend sh
```

### 数据库操作

```bash
# 备份数据库
docker-compose exec postgres pg_dump -U postgres QuestionBankDB > backup_$(date +%Y%m%d_%H%M%S).sql

# 恢复数据库
docker-compose exec -T postgres psql -U postgres QuestionBankDB < backup.sql

# 查看数据库连接
docker-compose exec postgres psql -U postgres -c "SELECT * FROM pg_stat_activity;"
```

### 更新部署

```bash
# 拉取最新代码
git pull

# 重新构建镜像
docker-compose build

# 重启服务（不中断）
docker-compose up -d
```

## 故障排查

### 1. 容器无法启动

```bash
# 查看详细日志
docker-compose logs

# 检查容器状态
docker-compose ps

# 重新构建
docker-compose build --no-cache
docker-compose up -d
```

### 2. 后端连接不上数据库

检查：
- 数据库容器是否正常运行: `docker-compose ps postgres`
- 环境变量是否配置正确
- 网络连接: `docker-compose exec backend ping postgres`

```bash
# 查看数据库日志
docker-compose logs postgres

# 检查数据库是否就绪
docker-compose exec postgres pg_isready -U postgres
```

### 3. 前端无法访问后端 API

检查：
- 环境变量 `VITE_API_BASE_URL` 是否正确
- 后端容器是否运行: `docker-compose ps backend`
- CORS 配置是否正确

```bash
# 测试后端 API
curl http://localhost:5000/health
```

### 4. 内存不足

```bash
# 查看容器资源使用
docker stats

# 增加 Docker 内存限制
# Docker Desktop: 设置 -> Resources -> Memory
```

### 5. 端口冲突

```bash
# 查看端口占用
netstat -tuln | grep :80
netstat -tuln | grep :5000
netstat -tuln | grep :5432

# 修改 docker-compose.yml 中的端口映射
```

### 6. 清理 Docker 资源

```bash
# 清理未使用的容器
docker container prune

# 清理未使用的镜像
docker image prune -a

# 清理未使用的卷
docker volume prune

# 清理所有（谨慎使用）
docker system prune -a --volumes
```

## 监控和维护

### 健康检查

所有服务都配置了健康检查：

```bash
# 查看健康状态
docker-compose ps
```

### 日志轮转

建议配置日志轮转，防止日志文件过大：

```bash
# 在 docker-compose.yml 中添加
logging:
  driver: "json-file"
  options:
    max-size: "10m"
    max-file: "3"
```

### 定期备份

建议设置 cron 任务定期备份数据库：

```bash
# 编辑 crontab
crontab -e

# 添加每天凌晨2点备份
0 2 * * * cd /path/to/TiKu && docker-compose exec -T postgres pg_dump -U postgres QuestionBankDB | gzip > backups/backup_$(date +\%Y\%m\%d).sql.gz
```

## 安全建议

1. **修改默认密码**: 必须修改 `.env` 中的所有默认密码
2. **使用 HTTPS**: 生产环境必须启用 HTTPS
3. **限制端口访问**: 使用防火墙限制对数据库端口的外部访问
4. **定期更新**: 定期更新 Docker 镜像和依赖包
5. **日志审计**: 定期检查日志文件
6. **备份策略**: 实施定期备份和灾难恢复计划

## 性能优化

1. **数据库连接池**: 已在后端配置
2. **静态资源缓存**: Nginx 已配置缓存头
3. **Gzip 压缩**: 已启用
4. **CDN**: 生产环境建议使用 CDN 加速静态资源

## 扩展部署

### 水平扩展后端

```bash
# 启动多个后端实例
docker-compose up -d --scale backend=3
```

需要配置负载均衡器（如 Nginx）来分发请求。

### 使用外部数据库

如果使用外部 PostgreSQL 数据库：

1. 在 `docker-compose.yml` 中移除 postgres 服务
2. 修改后端的 `ConnectionStrings__DefaultConnection`
3. 确保网络连接正常

## 许可证

MIT

## 技术支持

如遇问题，请查看：
- 项目 Issues: <repository-url>/issues
- 文档: README.md
- API 文档: http://localhost:5000/swagger
