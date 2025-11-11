#!/bin/bash

################################################################################
# 题库系统 Docker 一键部署脚本
# 支持开发环境和生产环境的完整部署流程
# Usage: ./deploy.sh [dev|prod] [options]
################################################################################

set -e

# ============================================================================
# 配置变量
# ============================================================================
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_NAME="question-bank"
VERSION="1.0.0"

# 颜色定义
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
PURPLE='\033[0;35m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

# 日志文件
LOG_DIR="${SCRIPT_DIR}/logs"
LOG_FILE="${LOG_DIR}/deploy_$(date +%Y%m%d_%H%M%S).log"

# ============================================================================
# 工具函数
# ============================================================================

# 创建日志目录
mkdir -p "${LOG_DIR}"

# 打印带颜色和时间戳的消息
log() {
    local level=$1
    shift
    local message="$@"
    local timestamp=$(date '+%Y-%m-%d %H:%M:%S')
    echo -e "${timestamp} ${message}" | tee -a "${LOG_FILE}"
}

print_header() {
    echo -e "${CYAN}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
    echo -e "${CYAN}  $1${NC}"
    echo -e "${CYAN}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
}

print_info() {
    log "INFO" "${GREEN}[✓]${NC} $1"
}

print_warn() {
    log "WARN" "${YELLOW}[!]${NC} $1"
}

print_error() {
    log "ERROR" "${RED}[✗]${NC} $1"
}

print_step() {
    log "STEP" "${BLUE}[→]${NC} $1"
}

print_success() {
    log "SUCCESS" "${GREEN}[✓✓✓]${NC} $1"
}

# 显示进度条
show_progress() {
    local duration=$1
    local steps=50
    local delay=$(echo "scale=2; $duration / $steps" | bc)

    echo -n "进度: ["
    for ((i=0; i<steps; i++)); do
        echo -n "="
        sleep $delay
    done
    echo "] 完成"
}

# ============================================================================
# 前置检查
# ============================================================================

check_command() {
    if ! command -v $1 &> /dev/null; then
        print_error "$1 未安装"
        echo ""
        case $1 in
            docker)
                echo "请访问 https://docs.docker.com/get-docker/ 安装 Docker"
                ;;
            docker-compose)
                echo "请访问 https://docs.docker.com/compose/install/ 安装 Docker Compose"
                ;;
            *)
                echo "请安装 $1"
                ;;
        esac
        exit 1
    fi
}

check_prerequisites() {
    print_header "检查系统环境"

    print_step "检查 Docker..."
    check_command docker
    DOCKER_VERSION=$(docker --version | awk '{print $3}' | sed 's/,//')
    print_info "Docker 版本: ${DOCKER_VERSION}"

    print_step "检查 Docker Compose..."
    check_command docker-compose
    COMPOSE_VERSION=$(docker-compose --version | awk '{print $4}' | sed 's/,//')
    print_info "Docker Compose 版本: ${COMPOSE_VERSION}"

    print_step "检查 Docker 守护进程..."
    if ! docker info &> /dev/null; then
        print_error "Docker 守护进程未运行"
        echo "请启动 Docker Desktop 或 Docker 服务"
        exit 1
    fi
    print_info "Docker 守护进程运行正常"

    # 检查系统资源
    print_step "检查系统资源..."

    # 检查可用内存
    if [[ "$OSTYPE" == "darwin"* ]]; then
        # macOS
        TOTAL_MEM=$(sysctl -n hw.memsize | awk '{print int($1/1024/1024/1024)}')
    else
        # Linux
        TOTAL_MEM=$(free -g | awk '/^Mem:/{print $2}')
    fi

    if [ "$TOTAL_MEM" -lt 4 ]; then
        print_warn "系统内存少于 4GB，可能影响性能"
    else
        print_info "系统内存: ${TOTAL_MEM}GB"
    fi

    # 检查磁盘空间
    AVAILABLE_SPACE=$(df -BG . | awk 'NR==2 {print $4}' | sed 's/G//')
    if [ "$AVAILABLE_SPACE" -lt 10 ]; then
        print_warn "可用磁盘空间少于 10GB"
    else
        print_info "可用磁盘空间: ${AVAILABLE_SPACE}GB"
    fi

    echo ""
}

# ============================================================================
# 环境配置
# ============================================================================

generate_random_password() {
    openssl rand -base64 32 | tr -d "=+/" | cut -c1-32
}

generate_jwt_secret() {
    openssl rand -base64 64 | tr -d "=+/" | cut -c1-64
}

setup_environment() {
    local env_mode=$1
    print_header "配置环境变量"

    if [ -f ".env" ]; then
        print_warn ".env 文件已存在"
        read -p "是否覆盖? (y/N): " -n 1 -r
        echo
        if [[ ! $REPLY =~ ^[Yy]$ ]]; then
            print_info "保留现有配置"
            return
        fi
    fi

    print_step "生成安全配置..."

    # 生成随机密码
    DB_PASSWORD=$(generate_random_password)
    JWT_SECRET=$(generate_jwt_secret)

    # 创建 .env 文件
    cat > .env << EOF
# ============================================================================
# 题库系统环境配置
# 生成时间: $(date '+%Y-%m-%d %H:%M:%S')
# 环境: ${env_mode}
# ============================================================================

# 数据库配置
POSTGRES_DB=QuestionBankDB
POSTGRES_USER=postgres
POSTGRES_PASSWORD=${DB_PASSWORD}

# JWT 配置
JWT_SECRET_KEY=${JWT_SECRET}
JWT_ISSUER=QuestionBankAPI
JWT_AUDIENCE=QuestionBankClient
JWT_EXPIRATION_MINUTES=1440

# 前端配置
EOF

    if [ "$env_mode" = "prod" ]; then
        echo "请输入生产环境的域名（例如: api.example.com）:"
        read -r API_DOMAIN
        if [ -z "$API_DOMAIN" ]; then
            API_DOMAIN="localhost:5000"
        fi
        echo "VITE_API_BASE_URL=https://${API_DOMAIN}/api" >> .env
    else
        echo "VITE_API_BASE_URL=http://localhost:5000/api" >> .env
    fi

    print_success ".env 文件创建成功"
    print_warn "数据库密码已生成，请妥善保管"

    # 显示配置摘要
    echo ""
    echo "配置摘要:"
    echo "  数据库: QuestionBankDB"
    echo "  数据库用户: postgres"
    echo "  JWT过期时间: 1440分钟 (24小时)"
    echo ""
}

# ============================================================================
# Docker 镜像构建
# ============================================================================

build_images() {
    local env_mode=$1
    print_header "构建 Docker 镜像"

    local compose_file="docker-compose.yml"
    if [ "$env_mode" = "prod" ]; then
        compose_file="docker-compose.prod.yml"
    fi

    print_step "清理旧镜像..."
    docker-compose -f ${compose_file} down --remove-orphans 2>/dev/null || true

    print_step "构建镜像（这可能需要几分钟）..."
    echo ""

    # 构建后端
    print_info "正在构建后端 API..."
    if docker-compose -f ${compose_file} build backend 2>&1 | tee -a "${LOG_FILE}"; then
        print_success "后端镜像构建完成"
    else
        print_error "后端镜像构建失败"
        exit 1
    fi

    echo ""

    # 构建前端
    print_info "正在构建前端应用..."
    if docker-compose -f ${compose_file} build frontend 2>&1 | tee -a "${LOG_FILE}"; then
        print_success "前端镜像构建完成"
    else
        print_error "前端镜像构建失败"
        exit 1
    fi

    echo ""
    print_success "所有镜像构建完成"
    echo ""
}

# ============================================================================
# 数据库初始化
# ============================================================================

wait_for_database() {
    print_step "等待数据库启动..."
    local max_attempts=30
    local attempt=0

    while [ $attempt -lt $max_attempts ]; do
        if docker-compose exec -T postgres pg_isready -U postgres &> /dev/null; then
            print_success "数据库已就绪"
            return 0
        fi

        attempt=$((attempt + 1))
        echo -n "."
        sleep 2
    done

    print_error "数据库启动超时"
    return 1
}

init_database() {
    print_header "初始化数据库"

    if ! wait_for_database; then
        print_error "数据库初始化失败"
        exit 1
    fi

    print_step "检查数据库连接..."
    if docker-compose exec -T postgres psql -U postgres -c '\l' &> /dev/null; then
        print_success "数据库连接正常"
    else
        print_error "无法连接到数据库"
        exit 1
    fi

    echo ""
}

# ============================================================================
# 服务健康检查
# ============================================================================

check_service_health() {
    local service=$1
    local url=$2
    local max_attempts=30
    local attempt=0

    print_step "检查 ${service} 服务健康状态..."

    while [ $attempt -lt $max_attempts ]; do
        if curl -f -s "${url}" > /dev/null 2>&1; then
            print_success "${service} 服务运行正常"
            return 0
        fi

        attempt=$((attempt + 1))
        echo -n "."
        sleep 2
    done

    print_warn "${service} 服务健康检查超时"
    return 1
}

verify_deployment() {
    print_header "验证部署"

    print_step "检查容器状态..."
    docker-compose ps
    echo ""

    # 检查后端
    check_service_health "后端API" "http://localhost:5000/health" || true

    # 检查前端
    check_service_health "前端应用" "http://localhost/" || true

    echo ""
}

# ============================================================================
# SSL 证书配置（生产环境）
# ============================================================================

setup_ssl() {
    print_header "配置 SSL 证书"

    mkdir -p nginx/ssl

    echo "请选择 SSL 证书配置方式:"
    echo "  1) 使用现有证书"
    echo "  2) 生成自签名证书（测试用）"
    echo "  3) 跳过（稍后手动配置）"
    read -p "请选择 (1-3): " -n 1 -r
    echo ""

    case $REPLY in
        1)
            echo "请提供证书路径:"
            read -p "证书文件 (.crt/.pem): " cert_path
            read -p "私钥文件 (.key): " key_path

            if [ -f "$cert_path" ] && [ -f "$key_path" ]; then
                cp "$cert_path" nginx/ssl/cert.pem
                cp "$key_path" nginx/ssl/key.pem
                print_success "证书配置完成"
            else
                print_error "证书文件不存在"
            fi
            ;;
        2)
            print_step "生成自签名证书..."
            openssl req -x509 -nodes -days 365 -newkey rsa:2048 \
                -keyout nginx/ssl/key.pem \
                -out nginx/ssl/cert.pem \
                -subj "/C=CN/ST=State/L=City/O=Organization/CN=localhost"
            print_success "自签名证书生成完成"
            print_warn "自签名证书仅用于测试，生产环境请使用正式证书"
            ;;
        3)
            print_info "跳过 SSL 配置"
            ;;
    esac

    echo ""
}

# ============================================================================
# 部署流程
# ============================================================================

deploy_dev() {
    print_header "部署开发环境"

    setup_environment "dev"
    build_images "dev"

    print_step "启动服务..."
    docker-compose up -d

    echo ""
    sleep 5

    init_database
    verify_deployment

    print_success "开发环境部署完成！"
    echo ""
    echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
    echo "  访问地址:"
    echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
    echo "  前端应用: ${CYAN}http://localhost${NC}"
    echo "  后端 API: ${CYAN}http://localhost:5000${NC}"
    echo "  API 文档: ${CYAN}http://localhost:5000/swagger${NC}"
    echo "  数据库:   ${CYAN}localhost:5432${NC}"
    echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
    echo ""
    echo "常用命令:"
    echo "  查看日志: ${YELLOW}docker-compose logs -f${NC}"
    echo "  停止服务: ${YELLOW}docker-compose down${NC}"
    echo "  重启服务: ${YELLOW}docker-compose restart${NC}"
    echo ""
}

deploy_prod() {
    print_header "部署生产环境"

    print_warn "生产环境部署需要额外配置"
    read -p "确认继续? (y/N): " -n 1 -r
    echo ""
    if [[ ! $REPLY =~ ^[Yy]$ ]]; then
        print_info "取消部署"
        exit 0
    fi

    setup_environment "prod"
    setup_ssl
    build_images "prod"

    print_step "启动服务..."
    docker-compose -f docker-compose.prod.yml up -d

    echo ""
    sleep 5

    init_database
    verify_deployment

    print_success "生产环境部署完成！"
    echo ""
    echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
    echo "  重要提醒:"
    echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
    echo "  1. 请妥善保管 .env 文件中的密码和密钥"
    echo "  2. 建议配置防火墙规则"
    echo "  3. 定期备份数据库"
    echo "  4. 监控服务运行状态"
    echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
    echo ""
    echo "常用命令:"
    echo "  查看日志: ${YELLOW}docker-compose -f docker-compose.prod.yml logs -f${NC}"
    echo "  停止服务: ${YELLOW}docker-compose -f docker-compose.prod.yml down${NC}"
    echo "  备份数据: ${YELLOW}./backup.sh create${NC}"
    echo ""
}

# ============================================================================
# 显示帮助
# ============================================================================

show_help() {
    cat << EOF
${CYAN}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}
  题库系统 Docker 部署脚本 v${VERSION}
${CYAN}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}

用法: ./deploy.sh [命令] [选项]

命令:
  dev         部署开发环境（默认）
  prod        部署生产环境
  stop        停止所有服务
  restart     重启所有服务
  status      查看服务状态
  logs        查看服务日志
  clean       清理所有容器和数据卷
  help        显示此帮助信息

选项:
  --no-build  跳过镜像构建
  --verbose   显示详细日志

示例:
  ${YELLOW}./deploy.sh${NC}              # 部署开发环境
  ${YELLOW}./deploy.sh dev${NC}          # 部署开发环境
  ${YELLOW}./deploy.sh prod${NC}         # 部署生产环境
  ${YELLOW}./deploy.sh stop${NC}         # 停止所有服务
  ${YELLOW}./deploy.sh status${NC}       # 查看服务状态

更多信息请参考: README.md
EOF
}

# ============================================================================
# 其他操作
# ============================================================================

stop_services() {
    print_header "停止服务"
    print_step "停止所有容器..."
    docker-compose down 2>/dev/null || true
    docker-compose -f docker-compose.prod.yml down 2>/dev/null || true
    print_success "服务已停止"
}

restart_services() {
    print_header "重启服务"
    print_step "重启所有容器..."
    docker-compose restart
    print_success "服务已重启"
}

show_status() {
    print_header "服务状态"
    docker-compose ps
}

show_logs() {
    print_header "服务日志"
    docker-compose logs -f --tail=100
}

clean_all() {
    print_header "清理环境"
    print_warn "此操作将删除所有容器、镜像和数据卷"
    read -p "确认继续? (y/N): " -n 1 -r
    echo ""
    if [[ ! $REPLY =~ ^[Yy]$ ]]; then
        print_info "取消清理"
        exit 0
    fi

    print_step "停止所有容器..."
    docker-compose down -v 2>/dev/null || true
    docker-compose -f docker-compose.prod.yml down -v 2>/dev/null || true

    print_step "删除相关镜像..."
    docker images | grep question-bank | awk '{print $3}' | xargs docker rmi -f 2>/dev/null || true

    print_success "清理完成"
}

# ============================================================================
# 主函数
# ============================================================================

main() {
    clear

    cat << "EOF"
  ___                  _   _           ___            _
 / _ \ _   _  ___  ___| |_(_) ___  _ _| _ ) __ _ _ _ | |__
| (_) | | | |/ _ \/ __| __| |/ _ \| '_ \| _ \/ _` | ' \| / /
 \__\_\ |_| |  __/\__ \ |_| | (_) | | | | _ \ (_| | || | \ \
      \__,_|\___||___/\__|_|\___/|_| |_|___/\__,_|_||_|_\_\

题库管理系统 - Docker 部署工具 v1.0.0
EOF
    echo ""

    # 解析参数
    local command=${1:-dev}

    case $command in
        dev)
            check_prerequisites
            deploy_dev
            ;;
        prod)
            check_prerequisites
            deploy_prod
            ;;
        stop)
            stop_services
            ;;
        restart)
            restart_services
            ;;
        status)
            show_status
            ;;
        logs)
            show_logs
            ;;
        clean)
            clean_all
            ;;
        help|--help|-h)
            show_help
            ;;
        *)
            print_error "未知命令: $command"
            echo ""
            show_help
            exit 1
            ;;
    esac

    echo ""
    print_info "日志已保存到: ${LOG_FILE}"
}

# 运行主函数
main "$@"
