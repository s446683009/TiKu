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
  Card,
  CardContent
} from '@mui/material';
import { Add, Edit, Delete, Visibility, NoteAlt } from '@mui/icons-material';
import apiService from '../api/apiService';
import type { QuestionNote } from '../types';
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

const NotesPage: React.FC = () => {
  const [notes, setNotes] = useState<QuestionNote[]>([]);
  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(10);
  const [totalCount, setTotalCount] = useState(0);

  useEffect(() => {
    loadNotes();
  }, [page, rowsPerPage]);

  const loadNotes = async () => {
    try {
      const response = await apiService.getNotes(page + 1, rowsPerPage);

      if (response.success && response.data) {
        const pageData = response.data as any;
        setNotes(pageData.items || []);
        setTotalCount(pageData.totalCount || 0);
      }
    } catch (error) {
      console.error('Failed to load notes:', error);
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
    return date.toLocaleDateString('zh-CN', {
      year: 'numeric',
      month: '2-digit',
      day: '2-digit'
    });
  };

  return (
    <Box>
      <Box display="flex" justifyContent="space-between" alignItems="center" mb={3}>
        <Typography variant="h4">我的笔记</Typography>
        <Button variant="contained" startIcon={<Add />}>
          添加笔记
        </Button>
      </Box>

      <Card sx={{ mb: 3 }}>
        <CardContent>
          <Box display="flex" alignItems="center" gap={2}>
            <NoteAlt color="primary" sx={{ fontSize: 40 }} />
            <Box>
              <Typography variant="h6">笔记统计</Typography>
              <Typography variant="body2" color="text.secondary">
                共记录 {totalCount} 条笔记
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
              <TableCell>笔记内容</TableCell>
              <TableCell>创建时间</TableCell>
              <TableCell>更新时间</TableCell>
              <TableCell>操作</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {notes.length === 0 ? (
              <TableRow>
                <TableCell colSpan={7} align="center">
                  <Typography variant="body2" color="text.secondary" py={3}>
                    暂无笔记���录
                  </Typography>
                </TableCell>
              </TableRow>
            ) : (
              notes.map((note) => (
                <TableRow key={note.id}>
                  <TableCell>
                    <Typography variant="body2" noWrap sx={{ maxWidth: 300 }}>
                      {note.question?.content || '题目已删除'}
                    </Typography>
                  </TableCell>
                  <TableCell>
                    {note.question ? (
                      <Chip
                        label={QuestionTypeName[note.question.type]}
                        size="small"
                      />
                    ) : (
                      '-'
                    )}
                  </TableCell>
                  <TableCell>
                    {note.question ? (
                      <Chip
                        label={DifficultyName[note.question.difficulty]}
                        size="small"
                        color={DifficultyColor[note.question.difficulty] as any}
                      />
                    ) : (
                      '-'
                    )}
                  </TableCell>
                  <TableCell>
                    <Typography variant="body2" noWrap sx={{ maxWidth: 300 }}>
                      {note.content}
                    </Typography>
                  </TableCell>
                  <TableCell>{formatDateTime(note.createdAt)}</TableCell>
                  <TableCell>{formatDateTime(note.updatedAt)}</TableCell>
                  <TableCell>
                    {note.question && (
                      <IconButton size="small" color="primary" title="查看题目">
                        <Visibility fontSize="small" />
                      </IconButton>
                    )}
                    <IconButton size="small" color="primary" title="编辑笔记">
                      <Edit fontSize="small" />
                    </IconButton>
                    <IconButton size="small" color="error" title="删除笔记">
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
          labelRowsPerPage="每��显示："
        />
      </TableContainer>
    </Box>
  );
};

export default NotesPage;
