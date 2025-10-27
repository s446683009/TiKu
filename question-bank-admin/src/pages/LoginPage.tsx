import React, { useState } from 'react';
import {
  Container,
  Paper,
  Box,
  TextField,
  Button,
  Typography,
  Alert,
  Tab,
  Tabs
} from '@mui/material';
import { useAuth } from '../contexts/AuthContext';
import { useNavigate } from 'react-router';
import apiService from '../api/apiService';

interface TabPanelProps {
  children?: React.ReactNode;
  index: number;
  value: number;
}

function TabPanel(props: TabPanelProps) {
  const { children, value, index, ...other } = props;
  return (
    <div hidden={value !== index} {...other}>
      {value === index && <Box sx={{ p: 3 }}>{children}</Box>}
    </div>
  );
}

const LoginPage: React.FC = () => {
  const [tabValue, setTabValue] = useState(0);
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [email, setEmail] = useState('');
  const [fullName, setFullName] = useState('');
  const [phone, setPhone] = useState('');
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [loading, setLoading] = useState(false);

  const { login } = useAuth();
  const navigate = useNavigate();

  const handleLogin = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setLoading(true);

    try {
      const success = await login(username, password);
      if (success) {
        navigate('/');
      } else {
        setError('用户名或密码错误');
      }
    } catch (err) {
      setError('登录失败，请重试');
    } finally {
      setLoading(false);
    }
  };

  const handleRegister = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setSuccess('');
    setLoading(true);

    try {
      const response = await apiService.register({
        username,
        password,
        email,
        fullName,
        phone
      });

      if (response.success) {
        setSuccess('注册成功！请登录');
        setTabValue(0);
        setPassword('');
      } else {
        setError(response.message || '注册失败');
      }
    } catch (err) {
      setError('注册失败，请重试');
    } finally {
      setLoading(false);
    }
  };

  return (
    <Container maxWidth="sm">
      <Box
        sx={{
          marginTop: 8,
          display: 'flex',
          flexDirection: 'column',
          alignItems: 'center'
        }}
      >
        <Paper elevation={3} sx={{ width: '100%', mt: 3 }}>
          <Box sx={{ borderBottom: 1, borderColor: 'divider' }}>
            <Tabs value={tabValue} onChange={(_, v) => setTabValue(v)} centered>
              <Tab label="登录" />
              <Tab label="注册" />
            </Tabs>
          </Box>

          <TabPanel value={tabValue} index={0}>
            <Typography component="h1" variant="h5" align="center" gutterBottom>
              题库管理系统
            </Typography>
            {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}
            {success && <Alert severity="success" sx={{ mb: 2 }}>{success}</Alert>}
            <Box component="form" onSubmit={handleLogin}>
              <TextField
                margin="normal"
                required
                fullWidth
                label="用户名"
                autoComplete="username"
                autoFocus
                value={username}
                onChange={(e) => setUsername(e.target.value)}
              />
              <TextField
                margin="normal"
                required
                fullWidth
                label="密码"
                type="password"
                autoComplete="current-password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
              />
              <Button
                type="submit"
                fullWidth
                variant="contained"
                sx={{ mt: 3, mb: 2 }}
                disabled={loading}
              >
                {loading ? '登录中...' : '登录'}
              </Button>
            </Box>
          </TabPanel>

          <TabPanel value={tabValue} index={1}>
            <Typography component="h1" variant="h5" align="center" gutterBottom>
              注册新账户
            </Typography>
            {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}
            {success && <Alert severity="success" sx={{ mb: 2 }}>{success}</Alert>}
            <Box component="form" onSubmit={handleRegister}>
              <TextField
                margin="normal"
                required
                fullWidth
                label="用户名"
                value={username}
                onChange={(e) => setUsername(e.target.value)}
              />
              <TextField
                margin="normal"
                required
                fullWidth
                label="邮箱"
                type="email"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
              />
              <TextField
                margin="normal"
                required
                fullWidth
                label="姓名"
                value={fullName}
                onChange={(e) => setFullName(e.target.value)}
              />
              <TextField
                margin="normal"
                fullWidth
                label="手机号"
                value={phone}
                onChange={(e) => setPhone(e.target.value)}
              />
              <TextField
                margin="normal"
                required
                fullWidth
                label="密码"
                type="password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
              />
              <Button
                type="submit"
                fullWidth
                variant="contained"
                sx={{ mt: 3, mb: 2 }}
                disabled={loading}
              >
                {loading ? '注册中...' : '注册'}
              </Button>
            </Box>
          </TabPanel>
        </Paper>
      </Box>
    </Container>
  );
};

export default LoginPage;
