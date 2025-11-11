# Nginx 反向代理部署说明

## 项目结构

```
nginx/
├── Dockerfile                          # Nginx Docker 镜像构建文件
├── nginx.conf                          # Nginx 主配置文件
├── conf.d/
│   ├── default.conf                   # 默认配置（已有）
│   └── lmsbt.plaza-network.cn.conf   # lmsbt 代理配置
├── ssl/
│   └── lmsbt.plaza-network.cn/       # SSL 证书目录
│       ├── fullchain.pem              # SSL 证书（需要手动添加）
│       └── privkey.pem                # SSL 私钥（需要手动添加）
└── logs/                              # 日志目录（自动创建）
```

## 快速开始

### 1. 准备 SSL 证书（HTTPS 必需）

如果需要启用 HTTPS (443端口)，请将 SSL 证书文件放置到以下位置：

```bash
# 创建证书目录
mkdir -p nginx/ssl/lmsbt.plaza-network.cn

# 复制证书文件（请替换为实际证书路径）
cp /path/to/your/fullchain.pem nginx/ssl/lmsbt.plaza-network.cn/
cp /path/to/your/privkey.pem nginx/ssl/lmsbt.plaza-network.cn/

# 设置证书文件权限
chmod 644 nginx/ssl/lmsbt.plaza-network.cn/fullchain.pem
chmod 600 nginx/ssl/lmsbt.plaza-network.cn/privkey.pem
```

### 2. 仅使用 HTTP (80端口)

如果暂时只需要 HTTP 访问，可以编辑 `nginx/conf.d/lmsbt.plaza-network.cn.conf`，删除或注释掉 443 端口的 server 配置块。

### 3. 构建并启动服务

```bash
# 使用 docker-compose 启动
docker-compose -f docker-compose.lmsbt.yml up -d

# 或者手动构建和运行
docker build -t lmsbt-nginx-proxy ./nginx
docker run -d \
  --name lmsbt-nginx-proxy \
  -p 80:80 \
  -p 443:443 \
  -v $(pwd)/nginx/ssl/lmsbt.plaza-network.cn:/etc/nginx/ssl/lmsbt.plaza-network.cn:ro \
  -v $(pwd)/nginx/logs:/var/log/nginx \
  lmsbt-nginx-proxy
```

### 4. 查看日志

```bash
# 查看所有日志
docker-compose -f docker-compose.lmsbt.yml logs -f

# 查看 nginx 访问日志
tail -f nginx/logs/lmsbt_access.log

# 查看 nginx 错误日志
tail -f nginx/logs/lmsbt_error.log
```

### 5. 测试配置

```bash
# 测试 nginx 配置是否正确
docker exec lmsbt-nginx-proxy nginx -t

# 重载配置（修改配置后）
docker exec lmsbt-nginx-proxy nginx -s reload
```

## 配置说明

### 代理目标
- **后端地址**: https://10.246.199.8/
- **域名**: lmsbt.plaza-network.cn
- **支持端口**: 80 (HTTP) 和 443 (HTTPS)

### 关键配置项

1. **SSL 验证**
   - `proxy_ssl_verify off` - 已关闭后端 SSL 证书验证
   - 适用于后端使用自签名证书的场景

2. **WebSocket 支持**
   - 已配置 WebSocket 升级头
   - 支持长连接和实时通信

3. **超时设置**
   - 连接超时: 60秒
   - 发送超时: 60秒
   - 读取超时: 60秒

4. **安全头**（HTTPS 模式）
   - HSTS
   - X-Frame-Options
   - X-Content-Type-Options
   - X-XSS-Protection

## 故障排查

### 1. 容器无法启动
```bash
# 查看容器日志
docker logs lmsbt-nginx-proxy

# 检查配置文件语法
docker run --rm -v $(pwd)/nginx/nginx.conf:/etc/nginx/nginx.conf:ro \
  nginx:1.25-alpine nginx -t -c /etc/nginx/nginx.conf
```

### 2. 无法访问后端
```bash
# 进入容器测试连通性
docker exec -it lmsbt-nginx-proxy sh
wget --no-check-certificate https://10.246.199.8/
```

### 3. SSL 证书问题
```bash
# 检查证书文件是否存在
docker exec lmsbt-nginx-proxy ls -la /etc/nginx/ssl/lmsbt.plaza-network.cn/

# 验证证书有效性
docker exec lmsbt-nginx-proxy openssl x509 -in /etc/nginx/ssl/lmsbt.plaza-network.cn/fullchain.pem -noout -text
```

## 停止和删除服务

```bash
# 停止服务
docker-compose -f docker-compose.lmsbt.yml stop

# 停止并删除容器
docker-compose -f docker-compose.lmsbt.yml down

# 删除容器和镜像
docker-compose -f docker-compose.lmsbt.yml down --rmi all
```

## 生产环境建议

1. **启用 HTTPS**: 强烈建议在生产环境中使用 HTTPS
2. **证书自动续期**: 使用 Let's Encrypt 配合 certbot 自动续期
3. **日志轮转**: 配置日志轮转避免日志文件过大
4. **监控告警**: 设置服务健康监控和告警
5. **限流配置**: 根据需要添加限流和防 DDoS 配置
