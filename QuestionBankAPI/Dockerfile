# 第一阶段：构建
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 复制解决方案和项目文件
COPY ["QuestionBankAPI/QuestionBank.API/QuestionBank.API.csproj", "QuestionBank.API/"]
COPY ["QuestionBankAPI/QuestionBank.Application/QuestionBank.Application.csproj", "QuestionBank.Application/"]
COPY ["QuestionBankAPI/QuestionBank.Domain/QuestionBank.Domain.csproj", "QuestionBank.Domain/"]
COPY ["QuestionBankAPI/QuestionBank.Infrastructure/QuestionBank.Infrastructure.csproj", "QuestionBank.Infrastructure/"]

# 还原依赖
RUN dotnet restore "QuestionBank.API/QuestionBank.API.csproj"

# 复制所有源代码
COPY QuestionBankAPI/ .

# 构建项目
WORKDIR "/src/QuestionBank.API"
RUN dotnet build "QuestionBank.API.csproj" -c Release -o /app/build

# 第二阶段：发布
FROM build AS publish
RUN dotnet publish "QuestionBank.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 第三阶段：运行时
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# 安装必要的工具（可选，用于调试）
# RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# 复制发布的文件
COPY --from=publish /app/publish .

# 暴露端口
EXPOSE 5000
EXPOSE 5001

# 设置环境变量
ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production

# 启动应用
ENTRYPOINT ["dotnet", "QuestionBank.API.dll"]
