# SSL 证书目录

请将 SSL 证书文件放置在此目录：

## 所需文件

1. **fullchain.pem** - 完整证书链
   - 包含服务器证书和中间证书

2. **privkey.pem** - 私钥文件
   - 证书对应的私钥

## 文件权限

```bash
chmod 644 fullchain.pem
chmod 600 privkey.pem
```

## 获取证书方式

### 方式 1: Let's Encrypt（推荐，免费）
```bash
certbot certonly --standalone -d lmsbt.plaza-network.cn
# 证书路径: /etc/letsencrypt/live/lmsbt.plaza-network.cn/
```

### 方式 2: 现有证书
直接复制证书文件到此目录

### 方式 3: 自签名证书（仅测试）
```bash
openssl req -x509 -nodes -days 365 -newkey rsa:2048 \
  -keyout privkey.pem \
  -out fullchain.pem \
  -subj "/CN=lmsbt.plaza-network.cn"
```

## 注意事项

- 生产环境必须使用受信任的 CA 签发的证书
- 定期续期证书（Let's Encrypt 证书有效期 90 天）
- 私钥文件请妥善保管，不要泄露
