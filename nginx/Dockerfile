FROM nginx:1.25-alpine

# 维护者信息
LABEL maintainer="your-email@example.com"
LABEL description="Nginx reverse proxy for lmsbt.plaza-network.cn"

# 删除默认配置
RUN rm -f /etc/nginx/conf.d/default.conf

# 复制 nginx 主配置文件
COPY nginx.conf /etc/nginx/nginx.conf

# 复制站点配置文件
COPY conf.d/*.conf /etc/nginx/conf.d/

# 创建 SSL 证书目录（用于挂载证书）
RUN mkdir -p /etc/nginx/ssl/lmsbt.plaza-network.cn

# 创建日志目录
RUN mkdir -p /var/log/nginx && \
    touch /var/log/nginx/lmsbt_access.log \
          /var/log/nginx/lmsbt_error.log \
          /var/log/nginx/lmsbt_ssl_access.log \
          /var/log/nginx/lmsbt_ssl_error.log

# 暴露端口
EXPOSE 80 443

# 健康检查
HEALTHCHECK --interval=30s --timeout=3s --start-period=10s --retries=3 \
    CMD wget --quiet --tries=1 --spider http://localhost/health || exit 1

# 启动 nginx
CMD ["nginx", "-g", "daemon off;"]
