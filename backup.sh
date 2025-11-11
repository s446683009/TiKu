#!/bin/bash

################################################################################
# 题库系统数据库备份和恢复脚本
# 支持自动备份、手动备份、恢复和清理功能
# Usage: ./backup.sh [create|restore|list|clean|auto] [options]
################################################################################

set -e

# ============================================================================
# 配置变量
# ============================================================================
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
BACKUP_DIR="${SCRIPT_DIR}/backups"
LOG_DIR="${SCRIPT_DIR}/logs"
LOG_FILE="${LOG_DIR}/backup_$(date +%Y%m%d_%H%M%S).log"

# Docker 容器名称
DB_CONTAINER="question-bank-db"
DB_CONTAINER_PROD="question-bank-db-prod"

# 数据库配置
DB_NAME="QuestionBankDB"
DB_USER="postgres"

# 备份保留策略
KEEP_DAILY=7      # 保留7天的每日备份
KEEP_WEEKLY=4     # 保留4周的每周备份
KEEP_MONTHLY=6    # 保留6个月的每月备份

# 颜色定义
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
NC='\033[0m'

# ============================================================================
# 工具函数
# ============================================================================

mkdir -p "${BACKUP_DIR}" "${LOG_DIR}"

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

# 获取容器名称
get_db_container() {
    if docker ps --format '{{.Names}}' | grep -q "${DB_CONTAINER_PROD}"; then
        echo "${DB_CONTAINER_PROD}"
    elif docker ps --format '{{.Names}}' | grep -q "${DB_CONTAINER}"; then
        echo "${DB_CONTAINER}"
    else
        print_error "数据库容器未运行"
        exit 1
    fi
}

# 格式化文件大小
format_size() {
    local size=$1
    if [ $size -lt 1024 ]; then
        echo "${size}B"
    elif [ $size -lt 1048576 ]; then
        echo "$(($size / 1024))KB"
    else
        echo "$(($size / 1048576))MB"
    fi
}

# ============================================================================
# 备份功能
# ============================================================================

create_backup() {
    local backup_type=${1:-manual}
    print_header "创建数据库备份"

    # 检查容器
    print_step "检查数据库容器..."
    local container=$(get_db_container)
    print_info "使用容器: ${container}"

    # 生成备份文件名
    local timestamp=$(date +%Y%m%d_%H%M%S)
    local backup_file="${BACKUP_DIR}/backup_${backup_type}_${timestamp}.sql"
    local compressed_file="${backup_file}.gz"

    print_step "开始备份数据库..."
    print_info "备份文件: ${compressed_file}"

    # 执行备份
    if docker exec ${container} pg_dump -U ${DB_USER} ${DB_NAME} | gzip > "${compressed_file}"; then
        local file_size=$(stat -f%z "${compressed_file}" 2>/dev/null || stat -c%s "${compressed_file}" 2>/dev/null)
        local formatted_size=$(format_size $file_size)

        print_info "备份完成，大小: ${formatted_size}"

        # 创建元数据文件
        cat > "${backup_file}.meta" << EOF
{
  "timestamp": "$(date '+%Y-%m-%d %H:%M:%S')",
  "type": "${backup_type}",
  "database": "${DB_NAME}",
  "container": "${container}",
  "size": ${file_size},
  "filename": "$(basename ${compressed_file})"
}
EOF

        # 验证备份
        print_step "验证备份文件..."
        if gunzip -t "${compressed_file}" 2>/dev/null; then
            print_info "备份文件完整性验证通过"
        else
            print_error "备份文件可能已损坏"
            return 1
        fi

        echo ""
        print_info "备份成功创建: ${compressed_file}"
        return 0
    else
        print_error "备份失败"
        return 1
    fi
}

# ============================================================================
# 恢复功能
# ============================================================================

list_backups() {
    print_header "可用的备份文件"

    if [ ! -d "${BACKUP_DIR}" ] || [ -z "$(ls -A ${BACKUP_DIR}/*.gz 2>/dev/null)" ]; then
        print_warn "没有找到备份文件"
        return 1
    fi

    echo ""
    printf "%-5s %-30s %-20s %-10s\n" "编号" "文件名" "创建时间" "大小"
    echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"

    local index=1
    for backup in $(ls -t ${BACKUP_DIR}/*.sql.gz); do
        local filename=$(basename ${backup})
        local filesize=$(stat -f%z "${backup}" 2>/dev/null || stat -c%s "${backup}" 2>/dev/null)
        local formatted_size=$(format_size $filesize)
        local filetime=$(stat -f%Sm -t "%Y-%m-%d %H:%M" "${backup}" 2>/dev/null || date -r ${backup} "+%Y-%m-%d %H:%M" 2>/dev/null)

        printf "%-5s %-30s %-20s %-10s\n" "$index" "$filename" "$filetime" "$formatted_size"
        index=$((index + 1))
    done

    echo ""
    return 0
}

restore_backup() {
    local backup_file=$1

    print_header "恢复数据库"

    # 如果没有指定文件，列出可用备份并让用户选择
    if [ -z "$backup_file" ]; then
        if ! list_backups; then
            exit 1
        fi

        read -p "请输入要恢复的备份编号（或文件路径）: " backup_input

        if [[ "$backup_input" =~ ^[0-9]+$ ]]; then
            # 用户输入编号
            local backup_array=($(ls -t ${BACKUP_DIR}/*.sql.gz))
            local index=$((backup_input - 1))

            if [ $index -lt 0 ] || [ $index -ge ${#backup_array[@]} ]; then
                print_error "无效的编号"
                exit 1
            fi

            backup_file="${backup_array[$index]}"
        else
            # 用户输入文件路径
            backup_file="$backup_input"
        fi
    fi

    # 检查备份文件
    if [ ! -f "$backup_file" ]; then
        print_error "备份文件不存在: ${backup_file}"
        exit 1
    fi

    print_info "备份文件: ${backup_file}"

    # 警告提示
    echo ""
    print_warn "警告: 恢复操作将覆盖当前数据库！"
    print_warn "建议先创建当前数据库的备份"
    echo ""
    read -p "是否继续? (yes/no): " confirm

    if [ "$confirm" != "yes" ]; then
        print_info "取消恢复操作"
        exit 0
    fi

    # 检查容器
    print_step "检查数据库容器..."
    local container=$(get_db_container)
    print_info "使用容器: ${container}"

    # 在恢复前创建安全备份
    print_step "创建安全备份..."
    create_backup "pre_restore"

    # 执行恢复
    print_step "开始恢复数据库..."

    # 断开所有活动连接
    print_step "断开数据库连接..."
    docker exec ${container} psql -U ${DB_USER} -c \
        "SELECT pg_terminate_backend(pid) FROM pg_stat_activity WHERE datname='${DB_NAME}' AND pid <> pg_backend_pid();" \
        2>/dev/null || true

    # 删除并重建数据库
    print_step "重建数据库..."
    docker exec ${container} psql -U ${DB_USER} -c "DROP DATABASE IF EXISTS ${DB_NAME};" 2>/dev/null || true
    docker exec ${container} psql -U ${DB_USER} -c "CREATE DATABASE ${DB_NAME};"

    # 恢复数据
    print_step "恢复数据..."
    if gunzip -c "${backup_file}" | docker exec -i ${container} psql -U ${DB_USER} ${DB_NAME} > /dev/null 2>&1; then
        echo ""
        print_info "数据库恢复成功！"
        print_info "恢复来源: ${backup_file}"
    else
        print_error "数据库恢复失败"
        print_error "可以使用安全备份进行恢复"
        exit 1
    fi
}

# ============================================================================
# 清理功能
# ============================================================================

clean_old_backups() {
    print_header "清理旧备份"

    if [ ! -d "${BACKUP_DIR}" ]; then
        print_info "备份目录不存在"
        return
    fi

    local total_backups=$(ls ${BACKUP_DIR}/*.sql.gz 2>/dev/null | wc -l)
    print_info "当前备份数量: ${total_backups}"

    if [ $total_backups -eq 0 ]; then
        print_info "没有备份需要清理"
        return
    fi

    # 获取当前日期
    local current_date=$(date +%s)

    # 删除超过保留期的每日备份
    print_step "清理每日备份（保留 ${KEEP_DAILY} 天）..."
    local daily_deleted=0
    for backup in ${BACKUP_DIR}/backup_daily_*.sql.gz ${BACKUP_DIR}/backup_manual_*.sql.gz; do
        if [ -f "$backup" ]; then
            local file_date=$(stat -f%B "$backup" 2>/dev/null || stat -c%Y "$backup" 2>/dev/null)
            local days_old=$(( (current_date - file_date) / 86400 ))

            if [ $days_old -gt $KEEP_DAILY ]; then
                rm -f "$backup" "$backup.meta"
                daily_deleted=$((daily_deleted + 1))
                print_info "删除: $(basename $backup) (${days_old} 天前)"
            fi
        fi
    done

    # 删除超过保留期的每周备份
    print_step "清理每周备份（保留 ${KEEP_WEEKLY} 周）..."
    local weekly_deleted=0
    for backup in ${BACKUP_DIR}/backup_weekly_*.sql.gz; do
        if [ -f "$backup" ]; then
            local file_date=$(stat -f%B "$backup" 2>/dev/null || stat -c%Y "$backup" 2>/dev/null)
            local weeks_old=$(( (current_date - file_date) / 604800 ))

            if [ $weeks_old -gt $KEEP_WEEKLY ]; then
                rm -f "$backup" "$backup.meta"
                weekly_deleted=$((weekly_deleted + 1))
                print_info "删除: $(basename $backup) (${weeks_old} 周前)"
            fi
        fi
    done

    # 删除超过保留期的每月备份
    print_step "清理每月备份（保留 ${KEEP_MONTHLY} 月）..."
    local monthly_deleted=0
    for backup in ${BACKUP_DIR}/backup_monthly_*.sql.gz; do
        if [ -f "$backup" ]; then
            local file_date=$(stat -f%B "$backup" 2>/dev/null || stat -c%Y "$backup" 2>/dev/null)
            local months_old=$(( (current_date - file_date) / 2592000 ))

            if [ $months_old -gt $KEEP_MONTHLY ]; then
                rm -f "$backup" "$backup.meta"
                monthly_deleted=$((monthly_deleted + 1))
                print_info "删除: $(basename $backup) (${months_old} 月前)"
            fi
        fi
    done

    local total_deleted=$((daily_deleted + weekly_deleted + monthly_deleted))

    echo ""
    if [ $total_deleted -gt 0 ]; then
        print_info "共删除 ${total_deleted} 个旧备份"
    else
        print_info "没有需要清理的备份"
    fi

    local remaining=$(ls ${BACKUP_DIR}/*.sql.gz 2>/dev/null | wc -l)
    print_info "剩余备份: ${remaining} 个"
}

# ============================================================================
# 自动备份
# ============================================================================

auto_backup() {
    print_header "自动备份任务"

    # 确定备份类型
    local day_of_week=$(date +%u)  # 1-7 (周一到周日)
    local day_of_month=$(date +%d)

    local backup_type="daily"
    if [ $day_of_month -eq 1 ]; then
        backup_type="monthly"
    elif [ $day_of_week -eq 7 ]; then
        backup_type="weekly"
    fi

    print_info "备份类型: ${backup_type}"

    # 执行备份
    if create_backup "${backup_type}"; then
        # 清理旧备份
        clean_old_backups
        print_info "自动备份完成"
        return 0
    else
        print_error "自动备份失败"
        return 1
    fi
}

# ============================================================================
# 显示帮助
# ============================================================================

show_help() {
    cat << EOF
${CYAN}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}
  题库系统数据库备份工具
${CYAN}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}

用法: ./backup.sh [命令] [选项]

命令:
  create          创建手动备份
  restore [file]  恢复数据库（不指定文件则交互选择）
  list            列出所有备份
  clean           清理旧备份
  auto            执行自动备份（用于 cron）
  help            显示此帮助信息

备份保留策略:
  每日备份: 保留 ${KEEP_DAILY} 天
  每周备份: 保留 ${KEEP_WEEKLY} 周
  每月备份: 保留 ${KEEP_MONTHLY} 个月

示例:
  ${YELLOW}./backup.sh create${NC}              # 创建手动备份
  ${YELLOW}./backup.sh restore${NC}             # 交互式恢复
  ${YELLOW}./backup.sh restore backup.sql.gz${NC}  # 恢复指定文件
  ${YELLOW}./backup.sh list${NC}                # 列出所有备份
  ${YELLOW}./backup.sh clean${NC}               # 清理旧备份

配置自动备份（crontab）:
  # 每天凌晨 2 点执行自动备份
  ${YELLOW}0 2 * * * cd $(pwd) && ./backup.sh auto${NC}

EOF
}

# ============================================================================
# 主函数
# ============================================================================

main() {
    local command=${1:-help}

    case $command in
        create)
            create_backup "manual"
            ;;
        restore)
            shift
            restore_backup "$1"
            ;;
        list)
            list_backups
            ;;
        clean)
            clean_old_backups
            ;;
        auto)
            auto_backup
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
