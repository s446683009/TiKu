#!/bin/bash

################################################################################
# 题库系统服务监控和健康检查脚本
# 监���服务状态、资源使用、日志错误等
# Usage: ./monitor.sh [status|health|logs|resources|alerts]
################################################################################

set -e

# ============================================================================
# 配置变量
# ============================================================================
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
LOG_DIR="${SCRIPT_DIR}/logs"
ALERT_LOG="${LOG_DIR}/alerts_$(date +%Y%m%d).log"

# 容器名称
CONTAINERS=("question-bank-db" "question-bank-api" "question-bank-frontend" "question-bank-nginx")

# 告警阈值
CPU_THRESHOLD=80
MEMORY_THRESHOLD=80
DISK_THRESHOLD=85
ERROR_COUNT_THRESHOLD=10

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

mkdir -p "${LOG_DIR}"

print_header() {
    echo -e "${CYAN}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
    echo -e "${CYAN}  $1${NC}"
    echo -e "${CYAN}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
}

print_info() {
    echo -e "${GREEN}[✓]${NC} $1"
}

print_warn() {
    echo -e "${YELLOW}[!]${NC} $1"
}

print_error() {
    echo -e "${RED}[✗]${NC} $1"
}

print_step() {
    echo -e "${BLUE}[→]${NC} $1"
}

log_alert() {
    local timestamp=$(date '+%Y-%m-%d %H:%M:%S')
    echo "${timestamp} ALERT: $1" >> "${ALERT_LOG}"
}

# 获取容器状态
get_container_status() {
    local container=$1
    if docker ps --format '{{.Names}}' | grep -q "^${container}$"; then
        echo "running"
    elif docker ps -a --format '{{.Names}}' | grep -q "^${container}$"; then
        echo "stopped"
    else
        echo "not_found"
    fi
}

# 获取容器健康状态
get_container_health() {
    local container=$1
    local health=$(docker inspect --format='{{.State.Health.Status}}' ${container} 2>/dev/null || echo "no_healthcheck")
    echo "$health"
}

# ============================================================================
# 服务状态检查
# ============================================================================

check_service_status() {
    print_header "服务状态检查"
    echo ""

    printf "%-30s %-15s %-15s %-10s\n" "容器名称" "状态" "健康检查" "运行时间"
    echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"

    local all_running=true

    for container in "${CONTAINERS[@]}"; do
        local status=$(get_container_status "$container")
        local health=$(get_container_health "$container")
        local uptime="N/A"

        if [ "$status" = "running" ]; then
            uptime=$(docker ps --format '{{.Status}}' --filter "name=^${container}$" | sed 's/Up //')
            local status_color="${GREEN}"
            local status_icon="●"
        elif [ "$status" = "stopped" ]; then
            local status_color="${RED}"
            local status_icon="●"
            all_running=false
        else
            local status_color="${YELLOW}"
            local status_icon="○"
            all_running=false
        fi

        # 健康检查状态颜色
        case $health in
            healthy)
                local health_color="${GREEN}"
                ;;
            unhealthy)
                local health_color="${RED}"
                all_running=false
                ;;
            starting)
                local health_color="${YELLOW}"
                ;;
            *)
                local health_color="${NC}"
                ;;
        esac

        printf "%-30s ${status_color}%-15s${NC} ${health_color}%-15s${NC} %-10s\n" \
            "$container" "${status_icon} ${status}" "$health" "$uptime"
    done

    echo ""

    if $all_running; then
        print_info "所有服务运行正常"
    else
        print_error "部分服务异常"
        return 1
    fi

    return 0
}

# ============================================================================
# 健康检查
# ============================================================================

check_health() {
    print_header "服务健康检查"
    echo ""

    local all_healthy=true

    # 检查数据库
    print_step "检查数据库服务..."
    local db_container=$(get_container_status "question-bank-db")
    if [ "$db_container" = "running" ]; then
        if docker exec question-bank-db pg_isready -U postgres &> /dev/null; then
            print_info "数据库连接正常"
        else
            print_error "数据库连接失败"
            log_alert "数据库连接失败"
            all_healthy=false
        fi
    fi

    # 检查后端 API
    print_step "检查后端 API..."
    if curl -f -s http://localhost:5000/health > /dev/null 2>&1; then
        local response=$(curl -s http://localhost:5000/health)
        print_info "后端 API 响应正常"
        echo "  响应: ${response}"
    else
        print_error "后端 API 无响应"
        log_alert "后端 API 无响应"
        all_healthy=false
    fi

    # 检查前端
    print_step "检查前端应用..."
    if curl -f -s http://localhost/ > /dev/null 2>&1; then
        print_info "前端应用响应正常"
    else
        print_error "前端应用无响应"
        log_alert "前端应用无响应"
        all_healthy=false
    fi

    # 检查数据库连接数
    print_step "检查数据库连接数..."
    if [ "$db_container" = "running" ]; then
        local conn_count=$(docker exec question-bank-db psql -U postgres -t -c "SELECT count(*) FROM pg_stat_activity;" 2>/dev/null | tr -d ' ')
        if [ -n "$conn_count" ]; then
            print_info "当前数据库连接: ${conn_count}"
            if [ "$conn_count" -gt 50 ]; then
                print_warn "数据库连接数较高"
                log_alert "数据库连接数: ${conn_count}"
            fi
        fi
    fi

    echo ""

    if $all_healthy; then
        print_info "所有服务健康检查通过"
        return 0
    else
        print_error "部分服务健康检查失败"
        return 1
    fi
}

# ============================================================================
# 资源使用监控
# ============================================================================

check_resources() {
    print_header "资源使用情况"
    echo ""

    # Docker 容器资源统计
    print_step "容器资源使用:"
    echo ""

    printf "%-30s %-15s %-15s %-15s %-15s\n" "容器名称" "CPU %" "内存使用" "内存限制" "网络 I/O"
    echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"

    local has_alert=false

    for container in "${CONTAINERS[@]}"; do
        if [ "$(get_container_status $container)" = "running" ]; then
            # 获取容器统计信息
            local stats=$(docker stats --no-stream --format "{{.CPUPerc}}|{{.MemUsage}}|{{.NetIO}}" ${container} 2>/dev/null || echo "N/A|N/A|N/A")

            IFS='|' read -r cpu mem net <<< "$stats"

            # 提取 CPU 百分比数值
            local cpu_value=$(echo $cpu | sed 's/%//' | sed 's/\..*//')

            # CPU 告警
            if [ "$cpu" != "N/A" ] && [ ! -z "$cpu_value" ] && [ "$cpu_value" -gt $CPU_THRESHOLD ]; then
                cpu="${RED}${cpu}${NC}"
                print_warn "${container} CPU 使用率过高: ${cpu_value}%"
                log_alert "${container} CPU 使用率过高: ${cpu_value}%"
                has_alert=true
            fi

            # 内存使用
            local mem_usage=$(echo $mem | awk '{print $1}')
            local mem_limit=$(echo $mem | awk '{print $3}')

            printf "%-30s %-15s %-15s %-15s %-15s\n" "$container" "$cpu" "$mem_usage" "$mem_limit" "$net"
        fi
    done

    echo ""

    # 系统资源
    print_step "系统资源:"
    echo ""

    # 磁盘使用
    print_info "磁盘使用情况:"
    df -h | grep -E '^Filesystem|/$' | awk '{printf "  %-30s %-10s %-10s %-10s %s\n", $1, $2, $3, $4, $5}'

    local disk_usage=$(df / | awk 'NR==2 {print $5}' | sed 's/%//')
    if [ "$disk_usage" -gt $DISK_THRESHOLD ]; then
        print_warn "磁盘使用率过高: ${disk_usage}%"
        log_alert "磁盘使用率过高: ${disk_usage}%"
        has_alert=true
    fi

    echo ""

    # Docker 卷使用
    print_info "Docker 卷使用情况:"
    docker system df -v | grep -A 5 "Local Volumes" | tail -n +2

    echo ""

    if $has_alert; then
        print_warn "发现资源使用告警"
        return 1
    else
        print_info "资源使用正常"
        return 0
    fi
}

# ============================================================================
# 日志分析
# ============================================================================

analyze_logs() {
    local hours=${1:-1}
    print_header "日志分析（最近 ${hours} 小时）"
    echo ""

    local total_errors=0

    for container in "${CONTAINERS[@]}"; do
        if [ "$(get_container_status $container)" = "running" ]; then
            print_step "分析 ${container} 日志..."

            # 统计错误日志
            local error_count=$(docker logs --since=${hours}h ${container} 2>&1 | grep -iE 'error|exception|fatal|failed' | wc -l | tr -d ' ')

            if [ "$error_count" -gt 0 ]; then
                if [ "$error_count" -gt $ERROR_COUNT_THRESHOLD ]; then
                    print_error "发现 ${error_count} 条错误日志"
                    log_alert "${container} 发现 ${error_count} 条错误日志"

                    # 显示最新的 5 条错误
                    echo ""
                    echo "  最新错误日志:"
                    docker logs --since=${hours}h ${container} 2>&1 | grep -iE 'error|exception|fatal|failed' | tail -n 5 | sed 's/^/    /'
                    echo ""
                else
                    print_warn "发现 ${error_count} 条错误日志"
                fi
                total_errors=$((total_errors + error_count))
            else
                print_info "未发现错误日志"
            fi
        fi
    done

    echo ""
    echo "总计错误日志: ${total_errors} 条"
    echo ""

    if [ $total_errors -gt $ERROR_COUNT_THRESHOLD ]; then
        print_warn "错误日志数量较多，建议检查"
        return 1
    else
        print_info "日志状态良好"
        return 0
    fi
}

# ============================================================================
# 告警报告
# ============================================================================

show_alerts() {
    print_header "告警报告"
    echo ""

    if [ ! -f "${ALERT_LOG}" ]; then
        print_info "今日无告警记录"
        return 0
    fi

    local alert_count=$(wc -l < "${ALERT_LOG}" | tr -d ' ')

    if [ "$alert_count" -eq 0 ]; then
        print_info "今日无告警记录"
        return 0
    fi

    print_warn "今日告警数量: ${alert_count}"
    echo ""

    echo "最近 10 条告警:"
    echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
    tail -n 10 "${ALERT_LOG}" | sed 's/^/  /'
    echo ""

    # 告警统计
    print_step "告警类型统计:"
    echo ""
    awk '{$1=$2=""; print}' "${ALERT_LOG}" | sort | uniq -c | sort -rn | sed 's/^/  /'
    echo ""

    return 1
}

# ============================================================================
# 完整监控报告
# ============================================================================

full_report() {
    local timestamp=$(date '+%Y-%m-%d %H:%M:%S')
    local report_file="${LOG_DIR}/monitor_report_$(date +%Y%m%d_%H%M%S).txt"

    {
        echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
        echo "  题库系统监控报告"
        echo "  生成时间: ${timestamp}"
        echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
        echo ""

        check_service_status
        echo ""

        check_health
        echo ""

        check_resources
        echo ""

        analyze_logs 1
        echo ""

        show_alerts

    } | tee "${report_file}"

    print_info "完整报告已保存至: ${report_file}"
}

# ============================================================================
# 实时监控
# ============================================================================

live_monitor() {
    print_header "实时监控模式"
    print_info "按 Ctrl+C 退出"
    echo ""

    while true; do
        clear
        echo "题库系统实时监控 - $(date '+%Y-%m-%d %H:%M:%S')"
        echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
        echo ""

        # 服务状态
        echo "【服务状态】"
        for container in "${CONTAINERS[@]}"; do
            local status=$(get_container_status "$container")
            local health=$(get_container_health "$container")

            if [ "$status" = "running" ]; then
                echo -e "  ${GREEN}●${NC} ${container}: ${status} (${health})"
            else
                echo -e "  ${RED}●${NC} ${container}: ${status}"
            fi
        done
        echo ""

        # 资源使用
        echo "【资源使用】"
        docker stats --no-stream --format "table {{.Name}}\t{{.CPUPerc}}\t{{.MemUsage}}\t{{.NetIO}}" \
            $(echo ${CONTAINERS[@]} | tr ' ' '\n' | paste -sd ' ')
        echo ""

        # 最新日志
        echo "【最新日志】"
        docker logs --tail=5 question-bank-api 2>&1 | sed 's/^/  /'

        sleep 5
    done
}

# ============================================================================
# 显示帮助
# ============================================================================

show_help() {
    cat << EOF
${CYAN}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}
  题库系统监控工具
${CYAN}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}

用法: ./monitor.sh [命令] [选项]

命令:
  status          检查服务状态
  health          执行健康检查
  resources       检查资源使用
  logs [hours]    分析日志（默认最近1小时）
  alerts          查看告警报告
  report          生成完整监控报告
  live            实时监控模式
  help            显示此帮助信息

告警阈值:
  CPU 使用率: ${CPU_THRESHOLD}%
  内存使用率: ${MEMORY_THRESHOLD}%
  磁盘使用率: ${DISK_THRESHOLD}%
  错误日志数: ${ERROR_COUNT_THRESHOLD} 条/小时

示例:
  ${YELLOW}./monitor.sh status${NC}         # 检查服务状态
  ${YELLOW}./monitor.sh health${NC}         # 健康检查
  ${YELLOW}./monitor.sh resources${NC}      # 资源使用
  ${YELLOW}./monitor.sh logs 24${NC}        # 分析最近24小时日志
  ${YELLOW}./monitor.sh report${NC}         # 生成完整报告
  ${YELLOW}./monitor.sh live${NC}           # 实时监控

配置定期监控（crontab）:
  # 每小时执行监控并生成报告
  ${YELLOW}0 * * * * cd $(pwd) && ./monitor.sh report${NC}

EOF
}

# ============================================================================
# 主函数
# ============================================================================

main() {
    local command=${1:-status}
    shift || true

    case $command in
        status)
            check_service_status
            ;;
        health)
            check_health
            ;;
        resources)
            check_resources
            ;;
        logs)
            analyze_logs ${1:-1}
            ;;
        alerts)
            show_alerts
            ;;
        report)
            full_report
            ;;
        live)
            live_monitor
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
}

# 运行主函数
main "$@"
