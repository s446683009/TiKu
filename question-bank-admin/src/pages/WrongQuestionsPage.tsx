import React, { useState, useEffect } from 'react';
import {
  Box,
  Paper,
  Typography,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Chip,
  IconButton,
  TablePagination,
  Card,
  CardContent
} from '@mui/material';
import { Visibility, Delete, TrendingUp } from '@mui/icons-material';
import apiService from '../api/apiService';
import type { WrongQuestion } from '../types';
import { QuestionType, Difficulty } from '../types';

const QuestionTypeName: Record<QuestionType, string> = {
  [QuestionType.SingleChoice]: '单选题',
  [QuestionType.MultipleChoice]: '多选题',
  [QuestionType.TrueFalse]: '判断题',
  [QuestionType.FillBlank]: '填空题',
  [QuestionType.ShortAnswer]: '简答题',
  [QuestionType.Material]: '材料题'
};

const DifficultyName: Record<Difficulty, string> = {
  [Difficulty.VeryEasy]: '非常简单',
  [Difficulty.Easy]: '简单',
  [Difficulty.Medium]: '中等',
  [Difficulty.Hard]: '困难',
  [Difficulty.VeryHard]: '非常困难'
};

const DifficultyColor: Record<Difficulty, string> = {
  [Difficulty.VeryEasy]: 'success',
  [Difficulty.Easy]: 'info',
  [Difficulty.Medium]: 'warning',
  [Difficulty.Hard]: 'error',
  [Difficulty.VeryHard]: 'error'
};

const WrongQuestionsPage: React.FC = () => {
  const [wrongQuestions, setWrongQuestions] = useState<WrongQuestion[]>([]);
  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(10);
  const [totalCount, setTotalCount] = useState(0);

  useEffect(() => {
    loadWrongQuestions();
  }, [page, rowsPerPage]);

  const loadWrongQuestions = async () => {
    try {
      const response = await apiService.getWrongQuestions(page + 1, rowsPerPage);

      if (response.success && response.data) {
        const pageData = response.data as any;
        setWrongQuestions(pageData.items || []);
        setTotalCount(pageData.totalCount || 0);
      }
    } catch (error) {
      console.error('Failed to load wrong questions:', error);
    }
  };

  const handleChangePage = (_: unknown, newPage: number) => {
    setPage(newPage);
  };

  const handleChangeRowsPerPage = (event: React.ChangeEvent<HTMLInputElement>) => {
    setRowsPerPage(parseInt(event.target.value, 10));
    setPage(0);
  };

  const formatDateTime = (dateStr: string) => {
    const date = new Date(dateStr);
    return date.toLocaleString('zh-CN', {
      year: 'numeric',
      month: '2-digit',
      day: '2-digit',
      hour: '2-digit',
      minute: '2-digit'
    });
  };

  return (
    <Box>
      <Typography variant="h4" mb={3}>
        我的错题本
      </Typography>

      <Card sx={{ mb: 3 }}>
        <CardContent>
          <Box display="flex" alignItems="center" gap={2}>
            <TrendingUp color="error" sx={{ fontSize: 40 }} />
            <Box>
              <Typography variant="h6">错题统计</Typography>
              <Typography variant="body2" color="text.secondary">
                共收录 {totalCount} 道错题，继续加油！
              </Typography>
            </Box>
          </Box>
        </CardContent>
      </Card>

      <TableContainer component={Paper}>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell>题干</TableCell>
              <TableCell>类型</TableCell>
              <TableCell>难度</TableCell>
              <TableCell>错误次数</TableCell>
              <TableCell>最近错误时间</TableCell>
              <TableCell>操作</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {wrongQuestions.length === 0 ? (
              <TableRow>
                <TableCell colSpan={6} align="center">
                  <Typography variant="body2" color="text.secondary" py={3}>
                    暂无错题记录，继续保持！
                  </Typography>
                </TableCell>
              </TableRow>
            ) : (
              wrongQuestions.map((wrongQuestion) => (
                <TableRow key={wrongQuestion.id}>
                  <TableCell>
                    <Typography variant="body2" noWrap sx={{ maxWidth: 400 }}>
                      {wrongQuestion.question.content}
                    </Typography>
                  </TableCell>
                  <TableCell>
                    <Chip
                      label={QuestionTypeName[wrongQuestion.question.type]}
                      size="small"
                    />
                  </TableCell>
                  <TableCell>
                    <Chip
                      label={DifficultyName[wrongQuestion.question.difficulty]}
                      size="small"
                      color={DifficultyColor[wrongQuestion.question.difficulty] as any}
                    />
                  </TableCell>
                  <TableCell>
                    <Chip
                      label={`${wrongQuestion.wrongCount}次`}
                      size="small"
                      color="error"
                    />
                  </TableCell>
                  <TableCell>
                    {formatDateTime(wrongQuestion.lastWrongAt)}
                  </TableCell>
                  <TableCell>
                    <IconButton size="small" color="primary" title="查看详情">
                      <Visibility fontSize="small" />
                    </IconButton>
                    <IconButton size="small" color="error" title="移除">
                      <Delete fontSize="small" />
                    </IconButton>
                  </TableCell>
                </TableRow>
              ))
            )}
          </TableBody>
        </Table>
        <TablePagination
          component="div"
          count={totalCount}
          page={page}
          onPageChange={handleChangePage}
          rowsPerPage={rowsPerPage}
          onRowsPerPageChange={handleChangeRowsPerPage}
          labelRowsPerPage="每页显示："
        />
      </TableContainer>
    </Box>
  );
};

export default WrongQuestionsPage;
