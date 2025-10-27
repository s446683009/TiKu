import React, { useState, useEffect } from 'react';
import {
  Box,
  Paper,
  Typography,
  Button,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  IconButton,
  Chip
} from '@mui/material';
import { Add, Edit, Delete, Visibility, Assessment } from '@mui/icons-material';
import apiService from '../api/apiService';
import type { Exam } from '../types';

const ExamStatusName: Record<number, { label: string; color: 'default' | 'primary' | 'success' | 'error' | 'warning' }> = {
  0: { label: '未开始', color: 'default' },
  1: { label: '进行中', color: 'primary' },
  2: { label: '已结束', color: 'success' },
  3: { label: '已取消', color: 'error' }
};

const ExamsPage: React.FC = () => {
  const [exams, setExams] = useState<Exam[]>([]);

  useEffect(() => {
    loadExams();
  }, []);

  const loadExams = async () => {
    try {
      const response = await apiService.getExams();

      if (response.success && response.data) {
        const examsData = Array.isArray(response.data) ? response.data : [];
        setExams(examsData);
      }
    } catch (error) {
      console.error('Failed to load exams:', error);
    }
  };

  const getExamStatus = (exam: Exam): number => {
    const now = new Date();
    const startTime = exam.startTime ? new Date(exam.startTime) : null;
    const endTime = exam.endTime ? new Date(exam.endTime) : null;

    if (exam.status === 3) return 3; // 已取消

    if (startTime && now < startTime) return 0; // 未开始
    if (endTime && now > endTime) return 2; // 已结束
    if (startTime && now >= startTime) return 1; // 进行中

    return 0; // 默认未开始
  };

  const formatDateTime = (dateStr?: string) => {
    if (!dateStr) return '-';
    const date = new Date(dateStr);
    return date.toLocaleString('zh-CN', {
      year: 'numeric',
      month: '2-digit',
      day: '2-digit',
      hour: '2-digit',
      minute: '2-digit'
    });
  };

  const formatDuration = (minutes: number) => {
    const hours = Math.floor(minutes / 60);
    const mins = minutes % 60;
    if (hours > 0) {
      return `${hours}小时${mins > 0 ? mins + '分钟' : ''}`;
    }
    return `${mins}分钟`;
  };

  return (
    <Box>
      <Box display="flex" justifyContent="space-between" alignItems="center" mb={3}>
        <Typography variant="h4">考试管理</Typography>
        <Button variant="contained" startIcon={<Add />}>
          创建考试
        </Button>
      </Box>

      <TableContainer component={Paper}>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell>考试标题</TableCell>
              <TableCell>描述</TableCell>
              <TableCell>状态</TableCell>
              <TableCell>开始时间</TableCell>
              <TableCell>结束时间</TableCell>
              <TableCell>时长</TableCell>
              <TableCell>最大尝试次数</TableCell>
              <TableCell>操作</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {exams.length === 0 ? (
              <TableRow>
                <TableCell colSpan={8} align="center">
                  <Typography variant="body2" color="text.secondary" py={3}>
                    暂无考试数据
                  </Typography>
                </TableCell>
              </TableRow>
            ) : (
              exams.map((exam) => {
                const status = getExamStatus(exam);
                const statusInfo = ExamStatusName[status] || ExamStatusName[0];

                return (
                  <TableRow key={exam.id}>
                    <TableCell>
                      <Typography variant="body2" fontWeight="medium">
                        {exam.title}
                      </Typography>
                    </TableCell>
                    <TableCell>
                      <Typography variant="body2" noWrap sx={{ maxWidth: 250 }}>
                        {exam.description || '-'}
                      </Typography>
                    </TableCell>
                    <TableCell>
                      <Chip
                        label={statusInfo.label}
                        size="small"
                        color={statusInfo.color}
                      />
                    </TableCell>
                    <TableCell>{formatDateTime(exam.startTime)}</TableCell>
                    <TableCell>{formatDateTime(exam.endTime)}</TableCell>
                    <TableCell>{formatDuration(exam.duration)}</TableCell>
                    <TableCell align="center">{exam.maxAttempts}</TableCell>
                    <TableCell>
                      <IconButton size="small" color="primary" title="查看详情">
                        <Visibility fontSize="small" />
                      </IconButton>
                      <IconButton size="small" color="info" title="查看统计">
                        <Assessment fontSize="small" />
                      </IconButton>
                      <IconButton size="small" color="primary" title="编辑">
                        <Edit fontSize="small" />
                      </IconButton>
                      <IconButton size="small" color="error" title="删除">
                        <Delete fontSize="small" />
                      </IconButton>
                    </TableCell>
                  </TableRow>
                );
              })
            )}
          </TableBody>
        </Table>
      </TableContainer>
    </Box>
  );
};

export default ExamsPage;
