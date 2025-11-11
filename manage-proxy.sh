#!/bin/bash

# Nginx 代理服务管理脚本

set -e

COMPOSE_FILE="docker-compose.lmsbt.yml"
CONTAINER_NAME="lmsbt-nginx-proxy"
SSL_DIR="nginx/ssl/lmsbt.plaza-network.cn"

# 颜色输出
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

info() {
    echo -e "${GREEN}[INFO]${NC} $1"
}

warn() {
    echo -e "${YELLOW}[WARN]${NC} $1"
}

error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# 检查 SSL 证书
check_ssl() {
    if [ ! -f "$SSL_DIR/fullchain.pem" ] || [ ! -f "$SSL_DIR/privkey.pem" ]; then
        warn "SSL 证书文件不存在"
        warn "HTTPS (443端口) 将无法使用"
        warn "请将证书文件放置到 $SSL_DIR 目录"
        echo ""
        read -p "是否继续？(y/N) " -n 1 -r
        echo
        if [[ ! $REPLY =~ ^[Yy]$ ]]; then
            exit 1
        fi
    else
        info "SSL 证书文件已找到"
    fi
}

# 启动服务
start() {
    info "正在启动 Nginx 反向代理服务..."
    check_ssl

    # 创建必要的目录
    mkdir -p nginx/logs

    docker-compose -f $COMPOSE_FILE up -d

    if [ $? -eq 0 ]; then
        info "服务启动成功！"
        info "HTTP:  http://lmsbt.plaza-network.cn"
        info "HTTPS: https://lmsbt.plaza-network.cn"
        echo ""
        info "查看日志: ./manage-proxy.sh logs"
        info "查看状态: ./manage-proxy.sh status"
    else
        error "服务启动失败"
        exit 1
    fi
}

# 停止服务
stop() {
    info "正在停止服务..."
    docker-compose -f $COMPOSE_FILE stop
    info "服务已停止"
}

# 重启服务
restart() {
    info "正在重启服务..."
    docker-compose -f $COMPOSE_FILE restart
    info "服务已重启"
}

# 查看状态
status() {
    info "服务状态:"
    docker-compose -f $COMPOSE_FILE ps
    echo ""

    if docker ps | grep -q $CONTAINER_NAME; then
        info "健康检查:"
        docker inspect --format='{{.State.Health.Status}}' $CONTAINER_NAME
    fi
}

# 查看日志
logs() {
    docker-compose -f $COMPOSE_FILE logs -f
}

# 测试配置
test_config() {
    info "测试 Nginx 配置..."
    if docker ps | grep -q $CONTAINER_NAME; then
        docker exec $CONTAINER_NAME nginx -t
    else
        error "容器未运行，无法测试配置"
        exit 1
    fi
}

# 重载配置
reload() {
    info "重载 Nginx 配置..."
    if docker ps | grep -q $CONTAINER_NAME; then
        docker exec $CONTAINER_NAME nginx -s reload
        info "配置重载成功"
    else
        error "容器未运行，无法重载配置"
        exit 1
    fi
}

# 构建镜像
build() {
    info "构建 Docker 镜像..."
    docker-compose -f $COMPOSE_FILE build
    info "镜像构建完成"
}

# 删除服务
down() {
    warn "这将停止并删除容器"
    read -p "确认继续？(y/N) " -n 1 -r
    echo
    if [[ $REPLY =~ ^[Yy]$ ]]; then
        docker-compose -f $COMPOSE_FILE down
        info "服务已删除"
    fi
}

# 完全清理
clean() {
    error "这将删除容器、镜像和卷"
    read -p "确认继续？(y/N) " -n 1 -r
    echo
    if [[ $REPLY =~ ^[Yy]$ ]]; then
        docker-compose -f $COMPOSE_FILE down --rmi all -v
        info "清理完成"
    fi
}

# 显示帮助
show_help() {
    cat << EOF
Nginx 反向代理服务管理脚本

用法: $0 [命令]

命令:
  start       启动服务
  stop        停止服务
  restart     重启服务
  status      查看服务状态
  logs        查看服务日志
  test        测试 Nginx 配置
  reload      重载 Nginx 配置（不停机）
  build       构建 Docker 镜像
  down        停止并删除服务
  clean       完全清理（包括镜像和卷）
  help        显示此帮助信息

示例:
  $0 start      # 启动服务
  $0 logs       # 查看日志
  $0 reload     # 重载配置

EOF
}

# 主程序
case "${1:-}" in
    start)
        start
        ;;
    stop)
        stop
        ;;
    restart)
        restart
        ;;
    status)
        status
        ;;
    logs)
        logs
        ;;
    test)
        test_config
        ;;
    reload)
        reload
        ;;
    build)
        build
        ;;
    down)
        down
        ;;
    clean)
        clean
        ;;
    help|--help|-h)
        show_help
        ;;
    *)
        error "未知命令: ${1:-}"
        echo ""
        show_help
        exit 1
        ;;
esac
