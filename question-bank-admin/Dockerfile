# 第一阶段：构建
FROM node:20-alpine AS build

WORKDIR /app

# 复制package文件
COPY package*.json ./

# 安装依赖
RUN npm ci --legacy-peer-deps

# 复制源代码
COPY . .

# 构建应用
RUN npm run build

# 第二阶段：生产运行
FROM nginx:alpine AS production

# 复制自定义nginx配置
COPY nginx.conf /etc/nginx/conf.d/default.conf

# 从构建阶段复制构建产物
COPY --from=build /app/dist /usr/share/nginx/html

# 暴露端口
EXPOSE 80

# 健康检查
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD wget --quiet --tries=1 --spider http://localhost/ || exit 1

# 启动nginx
CMD ["nginx", "-g", "daemon off;"]
