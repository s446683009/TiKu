import React, { useState } from 'react';
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
  TextField,
  Stack
} from '@mui/material';
import { Add, Edit, Block, CheckCircle, Search } from '@mui/icons-material';
import type { User } from '../types';
import { UserRole } from '../types';

const RoleName: Record<number, string> = {
  [UserRole.Student]: '学生',
  [UserRole.Teacher]: '教师',
  [UserRole.Admin]: '管理员'
};

const RoleColor: Record<number, 'default' | 'primary' | 'error'> = {
  [UserRole.Student]: 'default',
  [UserRole.Teacher]: 'primary',
  [UserRole.Admin]: 'error'
};

const UsersPage: React.FC = () => {
  const [users] = useState<User[]>([]);
  const [keyword, setKeyword] = useState('');

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
        <Typography variant="h4">用户管理</Typography>
        <Button variant="contained" startIcon={<Add />}>
          添加用户
        </Button>
      </Box>

      <Paper sx={{ p: 2, mb: 3 }}>
        <Stack direction="row" spacing={2} alignItems="center">
          <TextField
            fullWidth
            label="搜索用户（用户名/姓名/邮箱）"
            value={keyword}
            onChange={(e) => setKeyword(e.target.value)}
            size="small"
          />
          <Button variant="contained" startIcon={<Search />} sx={{ minWidth: 100 }}>
            搜索
          </Button>
        </Stack>
      </Paper>

      <TableContainer component={Paper}>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell>用户名</TableCell>
              <TableCell>姓名</TableCell>
              <TableCell>邮箱</TableCell>
              <TableCell>角色</TableCell>
              <TableCell>状态</TableCell>
              <TableCell>注册时间</TableCell>
              <TableCell>操作</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {users.length === 0 ? (
              <TableRow>
                <TableCell colSpan={7} align="center">
                  <Typography variant="body2" color="text.secondary" py={3}>
                    暂无用户数据，请连接后端API获取用户列表
                  </Typography>
                </TableCell>
              </TableRow>
            ) : (
              users.map((user) => (
                <TableRow key={user.id}>
                  <TableCell>
                    <Typography variant="body2" fontWeight="medium">
                      {user.username}
                    </Typography>
                  </TableCell>
                  <TableCell>{user.fullName}</TableCell>
                  <TableCell>{user.email}</TableCell>
                  <TableCell>
                    <Chip
                      label={RoleName[user.role] || '未知'}
                      size="small"
                      color={RoleColor[user.role] || 'default'}
                    />
                  </TableCell>
                  <TableCell>
                    <Chip
                      label={user.isActive ? '正常' : '已禁用'}
                      size="small"
                      color={user.isActive ? 'success' : 'error'}
                      icon={user.isActive ? <CheckCircle /> : <Block />}
                    />
                  </TableCell>
                  <TableCell>{formatDateTime(user.createdAt)}</TableCell>
                  <TableCell>
                    <IconButton size="small" color="primary" title="编辑">
                      <Edit fontSize="small" />
                    </IconButton>
                    <IconButton
                      size="small"
                      color={user.isActive ? 'error' : 'success'}
                      title={user.isActive ? '禁用' : '启用'}
                    >
                      {user.isActive ? <Block fontSize="small" /> : <CheckCircle fontSize="small" />}
                    </IconButton>
                  </TableCell>
                </TableRow>
              ))
            )}
          </TableBody>
        </Table>
      </TableContainer>
    </Box>
  );
};

export default UsersPage;
