#!/bin/bash

# 题库管理系统 Docker 快速启动脚本
# Usage: ./start.sh [dev|prod]

set -e

# 颜色定义
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# 打印带颜色的消息
print_info() {
    echo -e "${GREEN}[INFO]${NC} $1"
}

print_warn() {
    echo -e "${YELLOW}[WARN]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# 检查命令是否存在
check_command() {
    if ! command -v $1 &> /dev/null; then
        print_error "$1 未安装，请先安装 $1"
        exit 1
    fi
}

# 检查前置条件
check_prerequisites() {
    print_info "检查前置条件..."
    check_command docker
    check_command docker-compose

    # 检查 Docker 是否运行
    if ! docker info &> /dev/null; then
        print_error "Docker 未运行，请启动 Docker"
        exit 1
    fi

    print_info "前置条件检查通过 ✓"
}

# 检查环境变量文件
check_env_file() {
    if [ ! -f .env ]; then
        print_warn ".env 文件不存在，从 .env.example 创建..."
        if [ -f .env.example ]; then
            cp .env.example .env
            print_warn "请编辑 .env 文件配置必要的环境变量（数据库密码、JWT密钥等）"
            read -p "按 Enter 继续，或 Ctrl+C 退出先配置环境变量..."
        else
            print_error ".env.example 文件不存在"
            exit 1
        fi
    fi
}

# 启动开发环境
start_dev() {
    print_info "启动开发环境..."
    docker-compose up -d

    print_info ""
    print_info "开发环境启动完成！"
    print_info "前端地址: http://localhost"
    print_info "后端 API: http://localhost:5000"
    print_info "数据库: localhost:5432"
    print_info ""
    print_info "查看日志: docker-compose logs -f"
    print_info "停止服务: docker-compose down"
}

# 启动生产环境
start_prod() {
    print_info "启动生产环境..."

    # 检查生产环境必要配置
    if grep -q "your_secure_password_here" .env || grep -q "your_jwt_secret_key" .env; then
        print_error "生产环境必须修改默认密码和JWT密钥！"
        print_error "请编辑 .env 文件并修改以下配置："
        print_error "  - POSTGRES_PASSWORD"
        print_error "  - JWT_SECRET_KEY"
        exit 1
    fi

    docker-compose -f docker-compose.prod.yml up -d

    print_info ""
    print_info "生���环境启动完成！"
    print_info "前端地址: http://localhost"
    print_info "后端 API: http://localhost:5000"
    print_info ""
    print_info "查看日志: docker-compose -f docker-compose.prod.yml logs -f"
    print_info "停止服务: docker-compose -f docker-compose.prod.yml down"
}

# 显示帮助信息
show_help() {
    echo "题库管理系统 Docker 快速启动脚本"
    echo ""
    echo "用法: ./start.sh [选项]"
    echo ""
    echo "选项:"
    echo "  dev     启动开发环境（默认）"
    echo "  prod    启动生产环境"
    echo "  stop    停止所有服务"
    echo "  restart 重启所有服务"
    echo "  logs    查看日志"
    echo "  status  查看服务状态"
    echo "  help    显示帮助信息"
    echo ""
    echo "示例:"
    echo "  ./start.sh              # 启动开发环境"
    echo "  ./start.sh dev          # 启动开发环境"
    echo "  ./start.sh prod         # 启动生产环境"
    echo "  ./start.sh stop         # 停止服务"
}

# 停止服务
stop_services() {
    print_info "停止服务..."
    docker-compose down
    print_info "服务已停止"
}

# 重启服务
restart_services() {
    print_info "重启服务..."
    docker-compose restart
    print_info "服务已重启"
}

# 查看日志
show_logs() {
    docker-compose logs -f
}

# 查看状态
show_status() {
    docker-compose ps
}

# 主函数
main() {
    check_prerequisites
    check_env_file

    MODE=${1:-dev}

    case $MODE in
        dev)
            start_dev
            ;;
        prod)
            start_prod
            ;;
        stop)
            stop_services
            ;;
        restart)
            restart_services
            ;;
        logs)
            show_logs
            ;;
        status)
            show_status
            ;;
        help|--help|-h)
            show_help
            ;;
        *)
            print_error "未知选项: $MODE"
            show_help
            exit 1
            ;;
    esac
}

main "$@"
