#!/bin/bash

################################################################################
# 题库系统更新部署脚本
# 支持零停机更新、回滚、版本管理
# Usage: ./update.sh [update|rollback|version] [options]
################################################################################

set -e

# ============================================================================
# 配置变量
# ============================================================================
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
LOG_DIR="${SCRIPT_DIR}/logs"
BACKUP_DIR="${SCRIPT_DIR}/backups"
VERSION_FILE="${SCRIPT_DIR}/.deployed_version"
LOG_FILE="${LOG_DIR}/update_$(date +%Y%m%d_%H%M%S).log"

# 颜色定义
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
MAGENTA='\033[0;35m'
NC='\033[0m'

# ============================================================================
# 工具函数
# ============================================================================

mkdir -p "${LOG_DIR}" "${BACKUP_DIR}"

log() {
    local timestamp=$(date '+%Y-%m-%d %H:%M:%S')
    echo -e "${timestamp} $@" | tee -a "${LOG_FILE}"
}

print_header() {
    echo -e "${CYAN}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
    echo -e "${CYAN}  $1${NC}"
    echo -e "${CYAN}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
}

print_info() {
    log "${GREEN}[✓]${NC} $1"
}

print_warn() {
    log "${YELLOW}[!]${NC} $1"
}

print_error() {
    log "${RED}[✗]${NC} $1"
}

print_step() {
    log "${BLUE}[→]${NC} $1"
}

print_success() {
    log "${GREEN}[✓✓✓]${NC} $1"
}

# 获取当前部署版本
get_current_version() {
    if [ -f "${VERSION_FILE}" ]; then
        cat "${VERSION_FILE}"
    else
        echo "unknown"
    fi
}

# 保存版本信息
save_version() {
    local version=$1
    local timestamp=$(date '+%Y-%m-%d %H:%M:%S')
    echo "${version}" > "${VERSION_FILE}"
    echo "${timestamp}" >> "${VERSION_FILE}"
}

# 检查 Git 仓库
check_git_repo() {
    if [ ! -d ".git" ]; then
        print_warn "当前目录不是 Git 仓库"
        return 1
    fi
    return 0
}

# ============================================================================
# 更新前检查
# ============================================================================

pre_update_check() {
    print_header "更新前检查"

    local all_pass=true

    # 检查 Docker
    print_step "检查 Docker 环境..."
    if ! docker info &> /dev/null; then
        print_error "Docker 未运行"
        all_pass=false
    else
        print_info "Docker 环境正常"
    fi

    # 检查运行中的容器
    print_step "检查当前运行的容器..."
    local running_containers=$(docker ps --format '{{.Names}}' | grep -c "question-bank" || echo "0")
    if [ "$running_containers" -eq 0 ]; then
        print_warn "没有运行中的容器"
    else
        print_info "发现 ${running_containers} 个运行中的容器"
    fi

    # 检查磁盘空间
    print_step "检查磁盘空间..."
    local available_space=$(df -BG . | awk 'NR==2 {print $4}' | sed 's/G//')
    if [ "$available_space" -lt 5 ]; then
        print_warn "磁盘空间不足 5GB，可能影响更新"
        all_pass=false
    else
        print_info "可用磁盘空间: ${available_space}GB"
    fi

    # 检查是否有未提交的更改
    if check_git_repo; then
        print_step "检查 Git 状态..."
        if [ -n "$(git status --porcelain)" ]; then
            print_warn "存在未提交的更改"
            git status --short | sed 's/^/  /'
        else
            print_info "工作目录干净"
        fi
    fi

    echo ""

    if $all_pass; then
        print_info "所有检查通过"
        return 0
    else
        print_error "部分检查未通过"
        return 1
    fi
}

# ============================================================================
# 备份当前状态
# ============================================================================

backup_current_state() {
    print_header "备份当前状态"

    local backup_timestamp=$(date +%Y%m%d_%H%M%S)
    local backup_tag="backup_${backup_timestamp}"

    # 备份数据库
    print_step "备份数据库..."
    if [ -f "./backup.sh" ]; then
        if ./backup.sh create 2>&1 | tee -a "${LOG_FILE}"; then
            print_info "数据库备份完成"
        else
            print_error "数据库备份失败"
            return 1
        fi
    else
        print_warn "未找到 backup.sh 脚本，跳过数据库备份"
    fi

    # 保存当前镜像
    print_step "标记当前 Docker 镜像..."

    local images=("question-bank-api" "question-bank-frontend")
    for image in "${images[@]}"; do
        if docker images --format '{{.Repository}}' | grep -q "^${image}$"; then
            docker tag ${image}:latest ${image}:${backup_tag} 2>/dev/null || true
            print_info "已标记镜像: ${image}:${backup_tag}"
        fi
    done

    # 保存 Git commit
    if check_git_repo; then
        local current_commit=$(git rev-parse HEAD)
        echo "${current_commit}" > "${BACKUP_DIR}/commit_${backup_timestamp}.txt"
        print_info "当前 Git commit: ${current_commit:0:8}"
    fi

    # 保存版本信息
    echo "${backup_tag}" > "${BACKUP_DIR}/restore_point_${backup_timestamp}.txt"

    echo ""
    print_success "当前状态备份完成: ${backup_tag}"
    echo "${backup_tag}"
}

# ============================================================================
# 拉取最新代码
# ============================================================================

pull_latest_code() {
    print_header "拉取最新代码"

    if ! check_git_repo; then
        print_error "不是 Git 仓库，无法拉取代码"
        return 1
    fi

    # 获取当前分支
    local current_branch=$(git branch --show-current)
    print_info "当前分支: ${current_branch}"

    # 显示当前 commit
    local current_commit=$(git rev-parse HEAD)
    print_info "当前 commit: ${current_commit:0:8}"

    # 拉取最新代码
    print_step "拉取远程更新..."
    if git pull origin ${current_branch} 2>&1 | tee -a "${LOG_FILE}"; then
        local new_commit=$(git rev-parse HEAD)

        if [ "$current_commit" = "$new_commit" ]; then
            print_info "代码已是最新"
        else
            print_success "代码更新成功"
            print_info "新 commit: ${new_commit:0:8}"

            # 显示更新日志
            echo ""
            print_step "更新内容:"
            git log --oneline ${current_commit}..${new_commit} | sed 's/^/  /'
            echo ""
        fi

        return 0
    else
        print_error "代码拉取失败"
        return 1
    fi
}

# ============================================================================
# 重新构建镜像
# ============================================================================

rebuild_images() {
    local env_mode=${1:-prod}
    print_header "重新构建 Docker 镜像"

    local compose_file="docker-compose.yml"
    if [ "$env_mode" = "prod" ]; then
        compose_file="docker-compose.prod.yml"
    fi

    print_step "构建配置: ${compose_file}"

    # 构建后端
    print_step "构建后端镜像..."
    if docker-compose -f ${compose_file} build --no-cache backend 2>&1 | tee -a "${LOG_FILE}"; then
        print_success "后端镜像构建完成"
    else
        print_error "后端镜像构建失败"
        return 1
    fi

    # 构建前端
    print_step "构建前端镜像..."
    if docker-compose -f ${compose_file} build --no-cache frontend 2>&1 | tee -a "${LOG_FILE}"; then
        print_success "前端镜像构建完成"
    else
        print_error "前端镜像构建失败"
        return 1
    fi

    echo ""
    print_success "所有镜像构建完成"
    return 0
}

# ============================================================================
# 滚动更新
# ============================================================================

rolling_update() {
    local env_mode=${1:-prod}
    print_header "执行滚动更新"

    local compose_file="docker-compose.yml"
    if [ "$env_mode" = "prod" ]; then
        compose_file="docker-compose.prod.yml"
    fi

    # 更新后端
    print_step "更新后端服务..."
    if docker-compose -f ${compose_file} up -d --no-deps backend 2>&1 | tee -a "${LOG_FILE}"; then
        print_info "后端服务已启动"

        # 等待后端就绪
        print_step "等待后端服务就绪..."
        local max_attempts=30
        local attempt=0

        while [ $attempt -lt $max_attempts ]; do
            if curl -f -s http://localhost:5000/health > /dev/null 2>&1; then
                print_success "后端服务就绪"
                break
            fi

            attempt=$((attempt + 1))
            echo -n "."
            sleep 2
        done

        if [ $attempt -eq $max_attempts ]; then
            print_error "后端服务启动超时"
            return 1
        fi
    else
        print_error "后端服务更新失败"
        return 1
    fi

    echo ""

    # 更新前端
    print_step "更新前端服务..."
    if docker-compose -f ${compose_file} up -d --no-deps frontend 2>&1 | tee -a "${LOG_FILE}"; then
        print_info "前端服务已启动"

        # 等待前端就绪
        print_step "等待前端服务就绪..."
        sleep 5

        if curl -f -s http://localhost/ > /dev/null 2>&1; then
            print_success "前端服务就绪"
        else
            print_warn "前端服务可能未完全就绪"
        fi
    else
        print_error "前端服务更新失败"
        return 1
    fi

    echo ""
    print_success "滚动更新完成"
    return 0
}

# ============================================================================
# 更新后验证
# ============================================================================

post_update_verify() {
    print_header "更新后验证"

    local all_pass=true

    # 检查容器状态
    print_step "检查容器状态..."
    local containers=("question-bank-db" "question-bank-api" "question-bank-frontend")

    for container in "${containers[@]}"; do
        if docker ps --format '{{.Names}}' | grep -q "^${container}"; then
            print_info "${container}: 运行中"
        else
            print_error "${container}: 未运行"
            all_pass=false
        fi
    done

    echo ""

    # 健康检查
    print_step "执行健康检查..."

    # 检查后端
    if curl -f -s http://localhost:5000/health > /dev/null 2>&1; then
        print_info "后端 API: 正常"
    else
        print_error "后端 API: 异常"
        all_pass=false
    fi

    # 检查前端
    if curl -f -s http://localhost/ > /dev/null 2>&1; then
        print_info "前端应用: 正常"
    else
        print_error "前端应用: 异常"
        all_pass=false
    fi

    echo ""

    # 检查日志错误
    print_step "检查最近日志..."
    local error_count=$(docker logs --since=2m question-bank-api 2>&1 | grep -iE 'error|exception|fatal' | wc -l | tr -d ' ')

    if [ "$error_count" -gt 0 ]; then
        print_warn "发现 ${error_count} 条错误日志"
        docker logs --since=2m question-bank-api 2>&1 | grep -iE 'error|exception|fatal' | tail -n 5 | sed 's/^/  /'
    else
        print_info "未发现错误日志"
    fi

    echo ""

    if $all_pass; then
        print_success "所有验证通过"
        return 0
    else
        print_error "部分验证失败"
        return 1
    fi
}

# ============================================================================
# 完整更新流程
# ============================================================================

full_update() {
    local env_mode=${1:-prod}

    print_header "开始完整更新流程"
    print_info "环境: ${env_mode}"
    print_info "时间: $(date '+%Y-%m-%d %H:%M:%S')"
    echo ""

    # 1. 更新前检查
    if ! pre_update_check; then
        print_error "更新前检查失败"
        read -p "是否继续? (y/N): " -n 1 -r
        echo ""
        if [[ ! $REPLY =~ ^[Yy]$ ]]; then
            print_info "取消更新"
            exit 1
        fi
    fi

    echo ""

    # 2. 备份当前状态
    local backup_tag=$(backup_current_state)
    if [ -z "$backup_tag" ]; then
        print_error "备份失败"
        exit 1
    fi

    echo ""

    # 3. 拉取最新代码
    if ! pull_latest_code; then
        print_error "代码更新失败"
        exit 1
    fi

    echo ""

    # 4. 重新构建镜像
    if ! rebuild_images "$env_mode"; then
        print_error "镜像构建失败"
        print_warn "可以使用 './update.sh rollback' 回滚"
        exit 1
    fi

    echo ""

    # 5. 滚动更新
    if ! rolling_update "$env_mode"; then
        print_error "服务更新失败"
        print_warn "可以使用 './update.sh rollback' 回滚"
        exit 1
    fi

    echo ""

    # 6. 更新后验证
    if ! post_update_verify; then
        print_error "验证失败"
        print_warn "建议检查日志或回滚"
        read -p "是否立即回滚? (y/N): " -n 1 -r
        echo ""
        if [[ $REPLY =~ ^[Yy]$ ]]; then
            rollback_to_backup "$backup_tag"
        fi
        exit 1
    fi

    echo ""

    # 7. 保存新版本
    local new_version=$(git rev-parse HEAD 2>/dev/null || date +%Y%m%d_%H%M%S)
    save_version "$new_version"

    # 8. 清理旧镜像
    print_step "清理未使用的镜像..."
    docker image prune -f > /dev/null 2>&1 || true

    print_header "更新完成"
    print_success "系统已成功更新！"
    print_info "新版本: ${new_version:0:12}"
    print_info "备份标签: ${backup_tag}"
    echo ""
    echo "如需回滚，请执行:"
    echo "  ${YELLOW}./update.sh rollback${NC}"
    echo ""
}

# ============================================================================
# 回滚功能
# ============================================================================

rollback_to_backup() {
    local backup_tag=$1

    print_header "回滚到之前版本"

    if [ -z "$backup_tag" ]; then
        # 列出可用的备份
        print_step "可用的备份点:"
        echo ""

        local backups=($(docker images --format '{{.Tag}}' question-bank-api | grep "^backup_" || echo ""))

        if [ ${#backups[@]} -eq 0 ]; then
            print_error "没有找到可用的备份"
            return 1
        fi

        local index=1
        for backup in "${backups[@]}"; do
            echo "  $index) $backup"
            index=$((index + 1))
        done

        echo ""
        read -p "请选择要回滚的备份编号: " backup_index

        if [ -z "$backup_index" ] || [ "$backup_index" -lt 1 ] || [ "$backup_index" -gt ${#backups[@]} ]; then
            print_error "无效的选择"
            return 1
        fi

        backup_tag="${backups[$((backup_index - 1))]}"
    fi

    print_info "回滚到: ${backup_tag}"

    # 警告
    echo ""
    print_warn "警告: 回滚将停止当前服务并恢复到备份版本"
    read -p "确认继续? (yes/no): " confirm

    if [ "$confirm" != "yes" ]; then
        print_info "取消回滚"
        return 0
    fi

    echo ""

    # 回滚镜像
    print_step "恢复镜像..."

    if docker images --format '{{.Repository}}:{{.Tag}}' | grep -q "question-bank-api:${backup_tag}"; then
        docker tag question-bank-api:${backup_tag} question-bank-api:latest
        print_info "后端镜像已恢复"
    else
        print_warn "未找到后端备份镜像"
    fi

    if docker images --format '{{.Repository}}:{{.Tag}}' | grep -q "question-bank-frontend:${backup_tag}"; then
        docker tag question-bank-frontend:${backup_tag} question-bank-frontend:latest
        print_info "前端镜像已恢复"
    else
        print_warn "未找到前端备份镜像"
    fi

    # 重启服务
    print_step "重启服务..."
    docker-compose restart backend frontend 2>&1 | tee -a "${LOG_FILE}"

    # 验证
    echo ""
    if post_update_verify; then
        print_success "回滚成功"
    else
        print_error "回滚后验证失败，请检查服务状态"
        return 1
    fi
}

# ============================================================================
# 版本管理
# ============================================================================

show_version_info() {
    print_header "版本信息"

    # 当前部署版本
    if [ -f "${VERSION_FILE}" ]; then
        print_info "当前部署版本:"
        cat "${VERSION_FILE}" | sed 's/^/  /'
        echo ""
    else
        print_warn "未找到版本信息"
        echo ""
    fi

    # Git 信息
    if check_git_repo; then
        print_info "Git 信息:"
        echo "  分支: $(git branch --show-current)"
        echo "  Commit: $(git rev-parse HEAD)"
        echo "  最后更新: $(git log -1 --format='%ci')"
        echo "  作者: $(git log -1 --format='%an')"
        echo ""
    fi

    # Docker 镜像
    print_info "Docker 镜像:"
    docker images --format "table {{.Repository}}\t{{.Tag}}\t{{.Size}}\t{{.CreatedAt}}" | \
        grep -E "REPOSITORY|question-bank" | sed 's/^/  /'
    echo ""

    # 备份镜像
    print_info "可用备份:"
    docker images --format "  {{.Repository}}:{{.Tag}}" | grep "backup_" || echo "  无"
    echo ""
}

# ============================================================================
# 显示帮助
# ============================================================================

show_help() {
    cat << EOF
${CYAN}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}
  题库系统更新部署工具
${CYAN}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}

用法: ./update.sh [命令] [选项]

命令:
  update [env]    执行完整更新流程（env: dev/prod，默认 prod）
  rollback [tag]  回滚到指定备份点（不指定则交互选择）
  version         显示版本信息
  check           仅执行更新前检查
  help            显示此帮助信息

更新流程:
  1. 更新前检查（Docker、磁盘空间等）
  2. 备份当前状态（数据库、镜像）
  3. 拉取最新代码
  4. 重新构建镜像
  5. 滚动更新服务
  6. 更新后验证
  7. 保存版本信息

示例:
  ${YELLOW}./update.sh update${NC}           # 更新生产环境
  ${YELLOW}./update.sh update dev${NC}       # 更新开发环境
  ${YELLOW}./update.sh rollback${NC}         # 交互式回滚
  ${YELLOW}./update.sh version${NC}          # 查看版本信息
  ${YELLOW}./update.sh check${NC}            # 检查更新条件

注意事项:
  - 更新前会自动备份数据库和创建镜像备份
  - 支持零停机滚动更新
  - 更新失败可随时回滚
  - 建议在低峰期执行更新

EOF
}

# ============================================================================
# 主函数
# ============================================================================

main() {
    local command=${1:-help}
    shift || true

    case $command in
        update)
            full_update ${1:-prod}
            ;;
        rollback)
            rollback_to_backup "$1"
            ;;
        version)
            show_version_info
            ;;
        check)
            pre_update_check
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
