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
  Chip,
  IconButton,
  TablePagination,
  TextField,
  MenuItem,
  Stack
} from '@mui/material';
import { Add, Edit, Delete, Visibility } from '@mui/icons-material';
import apiService from '../api/apiService';
import type { Question } from '../types';
import { QuestionType, Difficulty } from '../types';
import Loading from '../components/Loading';
import ConfirmDialog from '../components/ConfirmDialog';
import QuestionDetailDialog from '../components/QuestionDetailDialog';

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

const QuestionsPage: React.FC = () => {
  const [questions, setQuestions] = useState<Question[]>([]);
  const [loading, setLoading] = useState(false);
  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(10);
  const [totalCount, setTotalCount] = useState(0);
  const [keyword, setKeyword] = useState('');
  const [selectedType, setSelectedType] = useState<number | ''>('');
  const [selectedDifficulty, setSelectedDifficulty] = useState<number | ''>('');
  const [selectedQuestion, setSelectedQuestion] = useState<Question | null>(null);
  const [detailDialogOpen, setDetailDialogOpen] = useState(false);
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
  const [questionToDelete, setQuestionToDelete] = useState<Question | null>(null);

  useEffect(() => {
    loadQuestions();
  }, [page, rowsPerPage]);

  const loadQuestions = async () => {
    setLoading(true);
    try {
      const response = await apiService.searchQuestions({
        keyword,
        type: selectedType !== '' ? selectedType : undefined,
        difficulty: selectedDifficulty !== '' ? selectedDifficulty : undefined,
        pageNumber: page + 1,
        pageSize: rowsPerPage
      });

      if (response.success && response.data) {
        const pageData = response.data as any;
        setQuestions(pageData.items || []);
        setTotalCount(pageData.totalCount || 0);
      }
    } catch (error) {
      console.error('Failed to load questions:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleSearch = () => {
    setPage(0);
    loadQuestions();
  };

  const handleChangePage = (_: unknown, newPage: number) => {
    setPage(newPage);
  };

  const handleChangeRowsPerPage = (event: React.ChangeEvent<HTMLInputElement>) => {
    setRowsPerPage(parseInt(event.target.value, 10));
    setPage(0);
  };

  const handleViewDetail = (question: Question) => {
    setSelectedQuestion(question);
    setDetailDialogOpen(true);
  };

  const handleDeleteClick = (question: Question) => {
    setQuestionToDelete(question);
    setDeleteDialogOpen(true);
  };

  const handleDeleteConfirm = async () => {
    if (!questionToDelete) return;

    try {
      const response = await apiService.deleteQuestion(questionToDelete.id);
      if (response.success) {
        loadQuestions();
      }
    } catch (error) {
      console.error('Failed to delete question:', error);
    } finally {
      setDeleteDialogOpen(false);
      setQuestionToDelete(null);
    }
  };

  return (
    <Box>
      <Box display="flex" justifyContent="space-between" alignItems="center" mb={3}>
        <Typography variant="h4">题目管理</Typography>
        <Button variant="contained" startIcon={<Add />}>
          创建题目
        </Button>
      </Box>

      <Paper sx={{ p: 2, mb: 3 }}>
        <Stack spacing={2}>
          <TextField
            fullWidth
            label="关键词搜索"
            value={keyword}
            onChange={(e) => setKeyword(e.target.value)}
            onKeyPress={(e) => e.key === 'Enter' && handleSearch()}
          />
          <Stack direction={{ xs: 'column', md: 'row' }} spacing={2}>
            <TextField
              fullWidth
              select
              label="题目类型"
              value={selectedType}
              onChange={(e) => setSelectedType(e.target.value as number | '')}
            >
              <MenuItem value="">全部</MenuItem>
              {Object.entries(QuestionTypeName).map(([value, label]) => (
                <MenuItem key={value} value={parseInt(value)}>{label}</MenuItem>
              ))}
            </TextField>
            <TextField
              fullWidth
              select
              label="难度"
              value={selectedDifficulty}
              onChange={(e) => setSelectedDifficulty(e.target.value as number | '')}
            >
              <MenuItem value="">全部</MenuItem>
              {Object.entries(DifficultyName).map(([value, label]) => (
                <MenuItem key={value} value={parseInt(value)}>{label}</MenuItem>
              ))}
            </TextField>
            <Button variant="contained" onClick={handleSearch} sx={{ minWidth: 100 }}>
              搜索
            </Button>
          </Stack>
        </Stack>
      </Paper>

      <TableContainer component={Paper}>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell>题干</TableCell>
              <TableCell>类型</TableCell>
              <TableCell>难度</TableCell>
              <TableCell>分数</TableCell>
              <TableCell>章节</TableCell>
              <TableCell>操作</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {loading ? (
              <TableRow>
                <TableCell colSpan={6}>
                  <Loading />
                </TableCell>
              </TableRow>
            ) : questions.length === 0 ? (
              <TableRow>
                <TableCell colSpan={6} align="center">
                  <Typography variant="body2" color="text.secondary" py={3}>
                    暂无题目数据
                  </Typography>
                </TableCell>
              </TableRow>
            ) : (
              questions.map((question) => (
                <TableRow key={question.id}>
                  <TableCell>
                    <Typography variant="body2" noWrap sx={{ maxWidth: 400 }}>
                      {question.content}
                    </Typography>
                  </TableCell>
                  <TableCell>
                    <Chip label={QuestionTypeName[question.type]} size="small" />
                  </TableCell>
                  <TableCell>
                    <Chip
                      label={DifficultyName[question.difficulty]}
                      size="small"
                      color={DifficultyColor[question.difficulty] as any}
                    />
                  </TableCell>
                  <TableCell>{question.score}</TableCell>
                  <TableCell>{question.chapter || '-'}</TableCell>
                  <TableCell>
                    <IconButton
                      size="small"
                      color="primary"
                      onClick={() => handleViewDetail(question)}
                      title="查看详情"
                    >
                      <Visibility fontSize="small" />
                    </IconButton>
                    <IconButton size="small" color="primary" title="编辑">
                      <Edit fontSize="small" />
                    </IconButton>
                    <IconButton
                      size="small"
                      color="error"
                      onClick={() => handleDeleteClick(question)}
                      title="删除"
                    >
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

      <QuestionDetailDialog
        open={detailDialogOpen}
        question={selectedQuestion}
        onClose={() => setDetailDialogOpen(false)}
      />

      <ConfirmDialog
        open={deleteDialogOpen}
        title="确认删除"
        message={`确定要删除题目"${questionToDelete?.content.substring(0, 30)}..."吗？此操作不可恢复。`}
        onConfirm={handleDeleteConfirm}
        onCancel={() => setDeleteDialogOpen(false)}
        confirmText="删除"
        confirmColor="error"
      />
    </Box>
  );
};

export default QuestionsPage;
