import React, { useState } from 'react';
import {
  Box,
  Paper,
  Typography,
  TextField,
  Button,
  Stack,
  Divider,
  Alert
} from '@mui/material';
import { Save, Lock, Person } from '@mui/icons-material';
import { useAuth } from '../contexts/AuthContext';

const SettingsPage: React.FC = () => {
  const { user } = useAuth();
  const [fullName, setFullName] = useState(user?.fullName || '');
  const [email, setEmail] = useState(user?.email || '');
  const [currentPassword, setCurrentPassword] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [message, setMessage] = useState<{ type: 'success' | 'error'; text: string } | null>(null);

  const handleSaveProfile = () => {
    setMessage({ type: 'success', text: '个人信息已更新（需要连接后端API）' });
    setTimeout(() => setMessage(null), 3000);
  };

  const handleChangePassword = () => {
    if (!currentPassword || !newPassword || !confirmPassword) {
      setMessage({ type: 'error', text: '请填写所有密码字段' });
      setTimeout(() => setMessage(null), 3000);
      return;
    }

    if (newPassword !== confirmPassword) {
      setMessage({ type: 'error', text: '新密码两次输入不一致' });
      setTimeout(() => setMessage(null), 3000);
      return;
    }

    if (newPassword.length < 6) {
      setMessage({ type: 'error', text: '新密码长度至少为6位' });
      setTimeout(() => setMessage(null), 3000);
      return;
    }

    setMessage({ type: 'success', text: '密码已修改（需要连接后端API）' });
    setCurrentPassword('');
    setNewPassword('');
    setConfirmPassword('');
    setTimeout(() => setMessage(null), 3000);
  };

  return (
    <Box>
      <Typography variant="h4" mb={3}>
        设置
      </Typography>

      {message && (
        <Alert severity={message.type} sx={{ mb: 3 }} onClose={() => setMessage(null)}>
          {message.text}
        </Alert>
      )}

      <Stack spacing={3}>
        <Paper sx={{ p: 3 }}>
          <Stack direction="row" alignItems="center" spacing={1} mb={2}>
            <Person color="primary" />
            <Typography variant="h6">个人信息</Typography>
          </Stack>

          <Stack spacing={2}>
            <TextField
              label="用户名"
              value={user?.username || ''}
              disabled
              fullWidth
              helperText="用户名不可修改"
            />

            <TextField
              label="姓名"
              value={fullName}
              onChange={(e) => setFullName(e.target.value)}
              fullWidth
            />

            <TextField
              label="邮箱"
              type="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              fullWidth
            />

            <Box>
              <Button
                variant="contained"
                startIcon={<Save />}
                onClick={handleSaveProfile}
              >
                保存信息
              </Button>
            </Box>
          </Stack>
        </Paper>

        <Paper sx={{ p: 3 }}>
          <Stack direction="row" alignItems="center" spacing={1} mb={2}>
            <Lock color="primary" />
            <Typography variant="h6">修改密码</Typography>
          </Stack>

          <Stack spacing={2}>
            <TextField
              label="当前密码"
              type="password"
              value={currentPassword}
              onChange={(e) => setCurrentPassword(e.target.value)}
              fullWidth
            />

            <Divider />

            <TextField
              label="新密码"
              type="password"
              value={newPassword}
              onChange={(e) => setNewPassword(e.target.value)}
              fullWidth
              helperText="密码长度至少为6位"
            />

            <TextField
              label="确认新密码"
              type="password"
              value={confirmPassword}
              onChange={(e) => setConfirmPassword(e.target.value)}
              fullWidth
            />

            <Box>
              <Button
                variant="contained"
                startIcon={<Lock />}
                onClick={handleChangePassword}
                color="secondary"
              >
                修改密码
              </Button>
            </Box>
          </Stack>
        </Paper>

        <Paper sx={{ p: 3 }}>
          <Typography variant="h6" mb={2}>
            账户信息
          </Typography>

          <Stack spacing={2}>
            <Box>
              <Typography variant="body2" color="text.secondary">
                角色
              </Typography>
              <Typography variant="body1">
                {user?.role === 3 ? '管理员' : user?.role === 2 ? '教师' : '学生'}
              </Typography>
            </Box>

            <Box>
              <Typography variant="body2" color="text.secondary">
                注册时间
              </Typography>
              <Typography variant="body1">
                {user?.createdAt
                  ? new Date(user.createdAt).toLocaleDateString('zh-CN', {
                      year: 'numeric',
                      month: 'long',
                      day: 'numeric'
                    })
                  : '-'}
              </Typography>
            </Box>

            <Box>
              <Typography variant="body2" color="text.secondary">
                账户状态
              </Typography>
              <Typography variant="body1" color={user?.isActive ? 'success.main' : 'error.main'}>
                {user?.isActive ? '正常' : '已禁用'}
              </Typography>
            </Box>
          </Stack>
        </Paper>
      </Stack>
    </Box>
  );
};

export default SettingsPage;
