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
  TablePagination,
  Chip
} from '@mui/material';
import { Add, Edit, Delete, Visibility } from '@mui/icons-material';
import apiService from '../api/apiService';
import type { Paper as PaperType } from '../types';

const PapersPage: React.FC = () => {
  const [papers, setPapers] = useState<PaperType[]>([]);
  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(10);
  const [totalCount, setTotalCount] = useState(0);

  useEffect(() => {
    loadPapers();
  }, [page, rowsPerPage]);

  const loadPapers = async () => {
    try {
      const response = await apiService.getPapers(page + 1, rowsPerPage);

      if (response.success && response.data) {
        const pageData = response.data as any;
        setPapers(pageData.items || []);
        setTotalCount(pageData.totalCount || 0);
      }
    } catch (error) {
      console.error('Failed to load papers:', error);
    }
  };

  const handleChangePage = (_: unknown, newPage: number) => {
    setPage(newPage);
  };

  const handleChangeRowsPerPage = (event: React.ChangeEvent<HTMLInputElement>) => {
    setRowsPerPage(parseInt(event.target.value, 10));
    setPage(0);
  };

  const handleDelete = async (id: string) => {
    if (window.confirm('确定要删除这份试卷吗？')) {
      try {
        const response = await apiService.deletePaper(id);
        if (response.success) {
          loadPapers();
        }
      } catch (error) {
        console.error('Failed to delete paper:', error);
      }
    }
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
        <Typography variant="h4">试卷管理</Typography>
        <Button variant="contained" startIcon={<Add />}>
          创建试卷
        </Button>
      </Box>

      <TableContainer component={Paper}>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell>试卷标题</TableCell>
              <TableCell>描述</TableCell>
              <TableCell>总分</TableCell>
              <TableCell>时长</TableCell>
              <TableCell>题目数量</TableCell>
              <TableCell>创建时间</TableCell>
              <TableCell>操作</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {papers.map((paper) => (
              <TableRow key={paper.id}>
                <TableCell>
                  <Typography variant="body2" fontWeight="medium">
                    {paper.title}
                  </Typography>
                </TableCell>
                <TableCell>
                  <Typography variant="body2" noWrap sx={{ maxWidth: 300 }}>
                    {paper.description || '-'}
                  </Typography>
                </TableCell>
                <TableCell>
                  <Chip label={`${paper.totalScore}分`} size="small" color="primary" />
                </TableCell>
                <TableCell>{formatDuration(paper.duration)}</TableCell>
                <TableCell>
                  <Chip label={`${paper.questionCount}题`} size="small" />
                </TableCell>
                <TableCell>
                  {new Date(paper.createdAt).toLocaleDateString('zh-CN')}
                </TableCell>
                <TableCell>
                  <IconButton size="small" color="primary" title="查看详情">
                    <Visibility fontSize="small" />
                  </IconButton>
                  <IconButton size="small" color="primary" title="编辑">
                    <Edit fontSize="small" />
                  </IconButton>
                  <IconButton
                    size="small"
                    color="error"
                    onClick={() => handleDelete(paper.id)}
                    title="删除"
                  >
                    <Delete fontSize="small" />
                  </IconButton>
                </TableCell>
              </TableRow>
            ))}
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

export default PapersPage;
