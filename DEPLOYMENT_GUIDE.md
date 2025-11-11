# 题库系统 Docker 部署完整指南

## 目录

- [系统概述](#系统概述)
- [快速开始](#快速开始)
- [部署脚本说明](#部署脚本说明)
- [环境配置](#环境配置)
- [部署流程](#部署流程)
- [运维管理](#运维管理)
- [故障排查](#故障排查)
- [安全建议](#安全建议)

## 系统概述

### 系统架构

```
┌─────────────────────────────────────────────────┐
│               Nginx (反向代理)                    │
│           HTTP/HTTPS (80/443)                    │
└────────────┬────────────────────────────────────┘
             │
    ┌────────┴────────┐
    │                 │
┌───▼────┐      ┌────▼──────┐
│  前端   │      │  后端 API  │
│ React  │      │  .NET 8   │
│ :80    │      │  :5000    │
└────────┘      └─────┬─────┘
                      │
                ┌─────▼──────┐
                │ PostgreSQL │
                │   :5432    │
                └────────────┘
```

### 技术栈

- **前端**: React + TypeScript + Vite
- **后端**: .NET 8 + ASP.NET Core
- **数据库**: PostgreSQL 16
- **反向代理**: Nginx
- **容器化**: Docker + Docker Compose

## 快速开始

### 前置要求

- Docker >= 20.10
- Docker Compose >= 2.0
- 4GB+ 可用内存
- 10GB+ 可用磁盘空间

### 一键部署

```bash
# 1. 克隆项目
git clone <your-repo-url>
cd TiKu

# 2. 赋予脚本执行权限
chmod +x *.sh

# 3. 部署开发环境
./deploy.sh dev

# 或部署生产环境
./deploy.sh prod
```

### 访问应用

部署完成后，访问：

- **前端应用**: http://localhost
- **后端 API**: http://localhost:5000
- **API 文档**: http://localhost:5000/swagger
- **数据库**: localhost:5432

## 部署脚本说明

本项目提供了完整的部署和运维脚本集：

### 1. deploy.sh - 一键部署脚本

完整的部署自动化脚本，包含所有部署流程。

```bash
# 部署开发环境
./deploy.sh dev

# 部署生产环境
./deploy.sh prod

# 停止服务
./deploy.sh stop

# 重启服务
./deploy.sh restart

# 查看状态
./deploy.sh status

# 查看日志
./deploy.sh logs

# 清理环境
./deploy.sh clean
```

**功能特性**:
- ✅ 自动检查系统环境和依赖
- ✅ 自动生成安全的密码和密钥
- ✅ 智能构建 Docker 镜像
- ✅ 自动初始化数据库
- ✅ 服务健康检查和验证
- ✅ 生产环境 SSL 证书配置
- ✅ 详细的部署日志记录

### 2. backup.sh - 数据库备份脚本

数据库备份和恢复管理工具。

```bash
# 创建手动备份
./backup.sh create

# 恢复数据库（交互式）
./backup.sh restore

# 恢复指定备份
./backup.sh restore /path/to/backup.sql.gz

# 列出所有备份
./backup.sh list

# 清理旧备份
./backup.sh clean

# 自动备份（用于 cron）
./backup.sh auto
```

**备份策略**:
- 每日备份：保留 7 天
- 每周备份：保留 4 周
- 每月备份：保留 6 个月

**配置自动备份**:

```bash
# 编辑 crontab
crontab -e

# 添加每天凌晨 2 点自动备份
0 2 * * * cd /path/to/TiKu && ./backup.sh auto
```

### 3. monitor.sh - 服务监控脚本

实时监控服务状态和资源使用。

```bash
# 检查服务状态
./monitor.sh status

# 健康检查
./monitor.sh health

# 检查资源使用
./monitor.sh resources

# 分析日志（最近1小时）
./monitor.sh logs

# 分析日志（最近24小时）
./monitor.sh logs 24

# 查看告警
./monitor.sh alerts

# 生成完整报告
./monitor.sh report

# 实时监控模式
./monitor.sh live
```

**监控指标**:
- 容器运行状态
- CPU 和内存使用
- 磁盘空间
- 网络 I/O
- 错误日志统计
- 数据库连接数

**配置定期监控**:

```bash
# 每小时执行监控检查
0 * * * * cd /path/to/TiKu && ./monitor.sh report
```

### 4. update.sh - 更新部署脚本

零停机更新和版本回滚工具。

```bash
# 更新生产环境
./update.sh update prod

# 更新开发环境
./update.sh update dev

# 回滚到之前版本
./update.sh rollback

# 查看版本信息
./update.sh version

# 检查更新条件
./update.sh check
```

**更新流程**:
1. 更新前检查（环境、资源）
2. 自动备份当前状态
3. 拉取最新代码
4. 重新构建镜像
5. 滚动更新服务
6. 健康检查验证
7. 保存版本信息

**回滚功能**:
- 自动创建回滚点
- 一键快速回滚
- 支持多版本管理

## 环境配置

### 环境变量文件 (.env)

部署脚本会自动生成 `.env` 文件，包含：

```bash
# 数据库配置
POSTGRES_DB=QuestionBankDB
POSTGRES_USER=postgres
POSTGRES_PASSWORD=<自动生成的安全密码>

# JWT 配置
JWT_SECRET_KEY=<自动生成的64位密钥>
JWT_ISSUER=QuestionBankAPI
JWT_AUDIENCE=QuestionBankClient
JWT_EXPIRATION_MINUTES=1440

# 前端配置
VITE_API_BASE_URL=http://localhost:5000/api  # 开发环境
# VITE_API_BASE_URL=https://api.yourdomain.com/api  # 生产环境
```

### Docker Compose 配置

项目提供两个 Docker Compose 配置文件：

**开发环境** (`docker-compose.yml`):
- 简化配置
- 开放所有端口便于调试
- 不包含资源限制

**生产环境** (`docker-compose.prod.yml`):
- 完整的安全配置
- 资源限制和优化
- 包含 Nginx 反向代理
- 日志和备份卷挂载

## 部署流程

### 开发环境部署

```bash
# 1. 执行部署脚本
./deploy.sh dev

# 2. 查看服务状态
./deploy.sh status

# 3. 查看日志
docker-compose logs -f

# 4. 访问应用
open http://localhost
```

### 生产环境部署

```bash
# 1. 配置域名和 SSL
# 编辑 .env 文件，设置生产域名

# 2. 执行生产部署
./deploy.sh prod

# 3. 配置 SSL 证书
# 脚本会引导你完成 SSL 配置

# 4. 验证部署
./monitor.sh health

# 5. 配置自动备份
crontab -e
# 添加: 0 2 * * * cd /path/to/TiKu && ./backup.sh auto

# 6. 配置监控
# 添加: 0 * * * * cd /path/to/TiKu && ./monitor.sh report
```

### 首次部署检查清单

- [ ] 服务器满足最低配置要求
- [ ] Docker 和 Docker Compose 已安装
- [ ] 防火墙开放必要端口（80, 443, 5000, 5432）
- [ ] 域名 DNS 已正确配置
- [ ] SSL 证书已准备好
- [ ] 环境变量已正确配置
- [ ] 数据库密码已修改为强密码
- [ ] JWT 密钥已生成
- [ ] 自动备份任务已配置
- [ ] 监控任务已配置

## 运维管理

### 日常运维命令

```bash
# 查看服务状态
docker-compose ps

# 查看实时日志
docker-compose logs -f

# 查看特定服务日志
docker-compose logs -f backend
docker-compose logs -f frontend

# 重启服务
docker-compose restart backend

# 进入容器
docker-compose exec backend sh
docker-compose exec postgres psql -U postgres -d QuestionBankDB

# 查看资源使用
docker stats
```

### 数据库管理

```bash
# 备份数据库
./backup.sh create

# 恢复数据库
./backup.sh restore

# 连接数据库
docker-compose exec postgres psql -U postgres -d QuestionBankDB

# 查看数据库大小
docker-compose exec postgres psql -U postgres -c "\l+"

# 查看表大小
docker-compose exec postgres psql -U postgres -d QuestionBankDB -c "\dt+"

# 导出数据
docker-compose exec postgres pg_dump -U postgres QuestionBankDB > backup.sql

# 导入数据
cat backup.sql | docker-compose exec -T postgres psql -U postgres QuestionBankDB
```

### 性能优化

#### 数据库优化

```sql
-- 查看慢查询
SELECT * FROM pg_stat_statements
ORDER BY mean_exec_time DESC
LIMIT 10;

-- 创建索引
CREATE INDEX idx_users_username ON users(username);
CREATE INDEX idx_questions_category ON questions(category);

-- 分析表
ANALYZE users;
ANALYZE questions;

-- 清理死元组
VACUUM ANALYZE;
```

#### 容器资源限制

编辑 `docker-compose.prod.yml`:

```yaml
services:
  backend:
    deploy:
      resources:
        limits:
          cpus: '2'
          memory: 2G
        reservations:
          cpus: '0.5'
          memory: 512M
```

### 日志管理

#### 配置日志轮转

编辑 `docker-compose.yml`:

```yaml
services:
  backend:
    logging:
      driver: "json-file"
      options:
        max-size: "10m"
        max-file: "3"
```

#### 查看日志

```bash
# 查看最近 100 行日志
docker-compose logs --tail=100 backend

# 实时跟踪日志
docker-compose logs -f backend

# 导出日志
docker-compose logs backend > backend.log

# 清理日志
docker-compose down && docker-compose up -d
```

## 故障排查

### 常见问题

#### 1. 容器无法启动

**症状**: `docker-compose ps` 显示容器退出

**排查步骤**:

```bash
# 查看容器日志
docker-compose logs backend

# 检查容器状态
docker-compose ps

# 重新构建
docker-compose build --no-cache backend
docker-compose up -d backend
```

#### 2. 后端无法连接数据库

**症状**: 后端日志显示数据库连接错误

**排查步骤**:

```bash
# 检查数据库容器
docker-compose ps postgres

# 测试数据库连接
docker-compose exec postgres pg_isready -U postgres

# 检查网络连接
docker-compose exec backend ping postgres

# 验证环境变量
docker-compose exec backend env | grep CONNECTION

# 查看数据库日志
docker-compose logs postgres
```

#### 3. 前端无法访问后端

**症状**: 浏览器显示网络错误

**排查步骤**:

```bash
# 检查后端健康状态
curl http://localhost:5000/health

# 检查 CORS 配置
docker-compose logs backend | grep CORS

# 验证环境变量
docker-compose exec frontend env | grep VITE_API

# 检查网络
docker network inspect question-bank-network
```

#### 4. 磁盘空间不足

```bash
# 查看磁盘使用
df -h

# 查看 Docker 空间使用
docker system df

# 清理未使用的资源
docker system prune -a --volumes

# 清理旧日志
find logs/ -name "*.log" -mtime +30 -delete

# 清理旧备份
find backups/ -name "*.sql.gz" -mtime +90 -delete
```

#### 5. 内存不足

```bash
# 查看内存使用
free -h

# 查看容器内存使用
docker stats

# 重启服务释放内存
docker-compose restart

# 调整容器内存限制
# 编辑 docker-compose.prod.yml
```

### 应急处理流程

#### 服务宕机

```bash
# 1. 快速重启
./deploy.sh restart

# 2. 检查服务状态
./monitor.sh health

# 3. 查看错误日志
./monitor.sh logs 1

# 4. 如果仍有问题，回滚
./update.sh rollback
```

#### 数据损坏

```bash
# 1. 停止服务
docker-compose down

# 2. 恢复最近的备份
./backup.sh restore

# 3. 重启服务
docker-compose up -d

# 4. 验证数据
./monitor.sh health
```

## 安全建议

### 1. 密码安全

```bash
# 使用强密码
POSTGRES_PASSWORD=$(openssl rand -base64 32)
JWT_SECRET_KEY=$(openssl rand -base64 64)

# 定期更换密码（每 90 天）
```

### 2. 网络安全

```bash
# 配置防火墙
sudo ufw enable
sudo ufw allow 80/tcp
sudo ufw allow 443/tcp
# 不要开放 5432 端口给外部

# 使用内部网络
# docker-compose.yml 中使用自定义网络
```

### 3. SSL/TLS 配置

```bash
# 使用 Let's Encrypt 免费证书
sudo certbot certonly --standalone -d yourdomain.com

# 自动续期
sudo certbot renew --dry-run

# 配置强加密套件（编辑 nginx 配置）
ssl_protocols TLSv1.2 TLSv1.3;
ssl_ciphers HIGH:!aNULL:!MD5;
```

### 4. 容器安全

- ✅ 使用非 root 用户运行容器
- ✅ 定期更新基础镜像
- ✅ 扫描镜像漏洞
- ✅ 限制容器资源
- ✅ 使用只读文件系统（where possible）

```bash
# 扫描镜像漏洞
docker scan question-bank-api:latest

# 更新基础镜像
docker-compose build --pull --no-cache
```

### 5. 数据备份

- ✅ 每日自动备份
- ✅ 异地备份存储
- ✅ 定期测试恢复
- ✅ 加密备份文件

```bash
# 加密备份
gpg --encrypt backup.sql.gz

# 上传到云存储
aws s3 cp backup.sql.gz.gpg s3://your-backup-bucket/
```

### 6. 审计和监控

```bash
# 启用审计日志
# 编辑 postgresql.conf
log_statement = 'all'
log_line_prefix = '%t [%p]: [%l-1] user=%u,db=%d '

# 配置告警
# 使用 ./monitor.sh 定期检查并发送告警邮件
```

## 监控和告警

### 配置监控仪表板

推荐使用 Grafana + Prometheus:

```yaml
# docker-compose.monitoring.yml
version: '3.8'

services:
  prometheus:
    image: prom/prometheus
    volumes:
      - ./prometheus.yml:/etc/prometheus/prometheus.yml
      - prometheus_data:/prometheus
    ports:
      - "9090:9090"

  grafana:
    image: grafana/grafana
    volumes:
      - grafana_data:/var/lib/grafana
    ports:
      - "3000:3000"
    environment:
      - GF_SECURITY_ADMIN_PASSWORD=admin

volumes:
  prometheus_data:
  grafana_data:
```

### 配置告警

使用脚本定期检查并发送告警：

```bash
#!/bin/bash
# alerts.sh

# 检查服务健康
if ! ./monitor.sh health > /dev/null 2>&1; then
    # 发送告警邮件或钉钉通知
    echo "服务健康检查失败" | mail -s "告警" admin@example.com
fi

# 检查磁盘空间
DISK_USAGE=$(df / | awk 'NR==2 {print $5}' | sed 's/%//')
if [ $DISK_USAGE -gt 85 ]; then
    echo "磁盘使用率: ${DISK_USAGE}%" | mail -s "磁盘告警" admin@example.com
fi
```

## 性能调优

### 数据库性能优化

```sql
-- 调整连接池
ALTER SYSTEM SET max_connections = 100;
ALTER SYSTEM SET shared_buffers = '256MB';
ALTER SYSTEM SET effective_cache_size = '1GB';
ALTER SYSTEM SET maintenance_work_mem = '64MB';
ALTER SYSTEM SET checkpoint_completion_target = 0.9;
ALTER SYSTEM SET wal_buffers = '16MB';
ALTER SYSTEM SET default_statistics_target = 100;
ALTER SYSTEM SET random_page_cost = 1.1;
ALTER SYSTEM SET effective_io_concurrency = 200;

-- 重启数据库使配置生效
docker-compose restart postgres
```

### 后端性能优化

```csharp
// 启用响应压缩
builder.Services.AddResponseCompression();

// 启用响应缓存
builder.Services.AddResponseCaching();

// 配置 Kestrel
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxConcurrentConnections = 100;
    options.Limits.MaxConcurrentUpgradedConnections = 100;
});
```

### 前端性能优化

```javascript
// vite.config.ts
export default defineConfig({
  build: {
    rollupOptions: {
      output: {
        manualChunks: {
          vendor: ['react', 'react-dom'],
          ui: ['antd']
        }
      }
    },
    chunkSizeWarningLimit: 1000
  }
});
```

## 扩展和高可用

### 水平扩展

```bash
# 扩展后端实例
docker-compose up -d --scale backend=3

# 配置 Nginx 负载均衡
upstream backend {
    server backend1:5000;
    server backend2:5000;
    server backend3:5000;
}
```

### 数据库主从复制

```yaml
# docker-compose.ha.yml
services:
  postgres-master:
    image: postgres:16-alpine
    environment:
      POSTGRES_REPLICATION_MODE: master
      POSTGRES_REPLICATION_USER: replicator
      POSTGRES_REPLICATION_PASSWORD: replica_password

  postgres-slave:
    image: postgres:16-alpine
    environment:
      POSTGRES_REPLICATION_MODE: slave
      POSTGRES_MASTER_HOST: postgres-master
      POSTGRES_REPLICATION_USER: replicator
      POSTGRES_REPLICATION_PASSWORD: replica_password
```

## 附录

### 脚本快速参考

```bash
# 部署
./deploy.sh dev          # 开发环境
./deploy.sh prod         # 生产环境

# 备份
./backup.sh create       # 创建备份
./backup.sh restore      # 恢复备份
./backup.sh list         # 列出备份

# 监控
./monitor.sh status      # 服务状态
./monitor.sh health      # 健康检查
./monitor.sh resources   # 资源使用
./monitor.sh live        # 实时监控

# 更新
./update.sh update prod  # 更新部署
./update.sh rollback     # 回滚版本
./update.sh version      # 查看版本
```

### 有用的命令

```bash
# Docker
docker-compose ps                    # 查看容器
docker-compose logs -f               # 查看日志
docker-compose restart               # 重启服务
docker-compose down                  # 停止服务
docker system prune -a --volumes     # 清理所有

# 数据库
docker-compose exec postgres psql -U postgres -d QuestionBankDB
\dt                                  # 列出表
\du                                  # 列出用户
\l                                   # 列出数据库

# 系统
df -h                                # 磁盘使用
free -h                              # 内存使用
top                                  # 进程监控
netstat -tuln                        # 端口监听
```

### 联系方式

如有问题，请联系：

- **技术支持**: support@example.com
- **紧急热线**: +86 xxx-xxxx-xxxx
- **文档**: https://docs.example.com
- **Issues**: https://github.com/your-repo/issues

---

**文档版本**: 1.0.0
**最后更新**: 2024-01-10
**维护者**: Your Team
