import React from 'react';
import {
  Stack,
  Paper,
  Typography,
  Box,
  Card,
  CardContent
} from '@mui/material';
import {
  Quiz,
  Description,
  School,
  People
} from '@mui/icons-material';
import { useAuth } from '../contexts/AuthContext';

interface StatCard {
  title: string;
  value: string;
  icon: React.ReactElement;
  color: string;
}

const DashboardPage: React.FC = () => {
  const { user } = useAuth();

  const stats: StatCard[] = [
    {
      title: '题目总数',
      value: '156',
      icon: <Quiz sx={{ fontSize: 40 }} />,
      color: '#1976d2'
    },
    {
      title: '试卷总数',
      value: '24',
      icon: <Description sx={{ fontSize: 40 }} />,
      color: '#2e7d32'
    },
    {
      title: '考试总数',
      value: '12',
      icon: <School sx={{ fontSize: 40 }} />,
      color: '#ed6c02'
    },
    {
      title: '用户总数',
      value: '89',
      icon: <People sx={{ fontSize: 40 }} />,
      color: '#9c27b0'
    }
  ];

  return (
    <Box>
      <Typography variant="h4" gutterBottom>
        欢迎回来，{user?.fullName}
      </Typography>
      <Typography variant="body1" color="text.secondary" paragraph>
        这是您的题库管理系统仪表板
      </Typography>

      <Stack
        direction={{ xs: 'column', sm: 'row' }}
        spacing={3}
        sx={{ mt: 2 }}
        flexWrap="wrap"
      >
        {stats.map((stat) => (
          <Card key={stat.title} sx={{ flex: { xs: '1 1 100%', sm: '1 1 calc(50% - 12px)', md: '1 1 calc(25% - 18px)' }, minWidth: 200 }}>
            <CardContent>
              <Box display="flex" justifyContent="space-between" alignItems="center">
                <Box>
                  <Typography color="text.secondary" gutterBottom>
                    {stat.title}
                  </Typography>
                  <Typography variant="h4">
                    {stat.value}
                  </Typography>
                </Box>
                <Box sx={{ color: stat.color }}>
                  {stat.icon}
                </Box>
              </Box>
            </CardContent>
          </Card>
        ))}
      </Stack>

      <Stack direction={{ xs: 'column', md: 'row' }} spacing={3} sx={{ mt: 3 }}>
        <Paper sx={{ p: 3, flex: 1 }}>
          <Typography variant="h6" gutterBottom>
            最近活动
          </Typography>
          <Typography variant="body2" color="text.secondary">
            暂无最近活动
          </Typography>
        </Paper>
        <Paper sx={{ p: 3, flex: 1 }}>
          <Typography variant="h6" gutterBottom>
            快速操作
          </Typography>
          <Typography variant="body2" color="text.secondary">
            使用左侧菜单开始管理您的题库
          </Typography>
        </Paper>
      </Stack>
    </Box>
  );
};

export default DashboardPage;
