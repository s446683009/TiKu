# ============================================================================
# 题库系统后端 API - 优化版 Dockerfile
# 多阶段构建，优化���缓存，减小镜像体积
# ============================================================================

# ============================================================================
# 第一阶段：基础镜像和依赖还原
# ============================================================================
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src

# 设置环境变量优化构建
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1 \
    DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1 \
    DOTNET_NOLOGO=1

# 仅复制项目文件用于依赖还原（优化 Docker 缓存）
COPY ["QuestionBank.API/QuestionBank.API.csproj", "QuestionBank.API/"]
COPY ["QuestionBank.Application/QuestionBank.Application.csproj", "QuestionBank.Application/"]
COPY ["QuestionBank.Domain/QuestionBank.Domain.csproj", "QuestionBank.Domain/"]
COPY ["QuestionBank.Infrastructure/QuestionBank.Infrastructure.csproj", "QuestionBank.Infrastructure/"]

# 还原依赖（单独层，充分利用缓存）
RUN dotnet restore "QuestionBank.API/QuestionBank.API.csproj" \
    --runtime alpine-x64 \
    --no-cache

# ============================================================================
# 第二阶段：构建应用
# ============================================================================
FROM build AS build-app

# 复制所有源代码
COPY . .

# 构建项目
WORKDIR "/src/QuestionBank.API"
RUN dotnet build "QuestionBank.API.csproj" \
    -c Release \
    -o /app/build \
    --no-restore

# ============================================================================
# 第三阶段：发布应用
# ============================================================================
FROM build-app AS publish
RUN dotnet publish "QuestionBank.API.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore \
    --no-build \
    /p:UseAppHost=false \
    /p:PublishTrimmed=false \
    /p:PublishReadyToRun=true

# ============================================================================
# 第四阶段：最终运行时镜像（最小化）
# ============================================================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS final

# 安全：创建非 root 用户
RUN addgroup -g 1000 appuser && \
    adduser -u 1000 -G appuser -s /bin/sh -D appuser

# 设置工作目录
WORKDIR /app

# 安装必要的运行时工具
RUN apk add --no-cache \
    curl \
    ca-certificates \
    tzdata && \
    rm -rf /var/cache/apk/*

# 设置时区
ENV TZ=Asia/Shanghai

# 创建日志目录并设置权限
RUN mkdir -p /app/logs && \
    chown -R appuser:appuser /app

# 从发布阶段复制文件
COPY --from=publish --chown=appuser:appuser /app/publish .

# 切换到非 root 用户
USER appuser

# 暴露端口
EXPOSE 5000

# 设置环境变量
ENV ASPNETCORE_URLS=http://+:5000 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

# 健康检查
HEALTHCHECK --interval=30s --timeout=3s --start-period=10s --retries=3 \
    CMD curl -f http://localhost:5000/health || exit 1

# 添加元数据标签
LABEL maintainer="your-email@example.com" \
      version="1.0" \
      description="Question Bank API Service"

# 启动应用
ENTRYPOINT ["dotnet", "QuestionBank.API.dll"]
