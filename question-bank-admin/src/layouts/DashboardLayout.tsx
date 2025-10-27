import React, { useState } from 'react';
import {
  Box,
  Drawer,
  AppBar,
  Toolbar,
  List,
  Typography,
  Divider,
  IconButton,
  ListItem,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  Avatar,
  Menu,
  MenuItem
} from '@mui/material';
import {
  Menu as MenuIcon,
  Dashboard,
  Quiz,
  Description,
  School,
  AccountTree,
  People,
  BookmarkBorder,
  Note,
  ErrorOutline,
  Logout,
  Settings
} from '@mui/icons-material';
import { useNavigate, Outlet } from 'react-router';
import { useAuth } from '../contexts/AuthContext';
import { UserRole } from '../types';

const drawerWidth = 240;

interface MenuItem {
  text: string;
  icon: React.ReactElement;
  path: string;
  roles?: UserRole[];
}

const menuItems: MenuItem[] = [
  { text: '仪表板', icon: <Dashboard />, path: '/' },
  { text: '题目管理', icon: <Quiz />, path: '/questions', roles: [UserRole.Teacher, UserRole.Admin] },
  { text: '试卷管理', icon: <Description />, path: '/papers', roles: [UserRole.Teacher, UserRole.Admin] },
  { text: '考试管理', icon: <School />, path: '/exams', roles: [UserRole.Teacher, UserRole.Admin] },
  { text: '知识点管理', icon: <AccountTree />, path: '/knowledge-points', roles: [UserRole.Teacher, UserRole.Admin] },
  { text: '用户管理', icon: <People />, path: '/users', roles: [UserRole.Admin] },
];

const learningItems: MenuItem[] = [
  { text: '我的收藏', icon: <BookmarkBorder />, path: '/favorites' },
  { text: '我的笔记', icon: <Note />, path: '/notes' },
  { text: '错题本', icon: <ErrorOutline />, path: '/wrong-questions' },
];

const DashboardLayout: React.FC = () => {
  const [mobileOpen, setMobileOpen] = useState(false);
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
  const navigate = useNavigate();
  const { user, logout, hasRole } = useAuth();

  const handleDrawerToggle = () => {
    setMobileOpen(!mobileOpen);
  };

  const handleMenuOpen = (event: React.MouseEvent<HTMLElement>) => {
    setAnchorEl(event.currentTarget);
  };

  const handleMenuClose = () => {
    setAnchorEl(null);
  };

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  const handleNavigate = (path: string) => {
    navigate(path);
    setMobileOpen(false);
  };

  const canAccessMenu = (item: MenuItem): boolean => {
    if (!item.roles || item.roles.length === 0) return true;
    return item.roles.some(role => hasRole(role));
  };

  const drawer = (
    <div>
      <Toolbar>
        <Typography variant="h6" noWrap component="div">
          题库系统
        </Typography>
      </Toolbar>
      <Divider />
      <List>
        {menuItems.filter(canAccessMenu).map((item) => (
          <ListItem key={item.text} disablePadding>
            <ListItemButton onClick={() => handleNavigate(item.path)}>
              <ListItemIcon>{item.icon}</ListItemIcon>
              <ListItemText primary={item.text} />
            </ListItemButton>
          </ListItem>
        ))}
      </List>
      <Divider />
      <Typography variant="subtitle2" sx={{ px: 2, py: 1, color: 'text.secondary' }}>
        学习中心
      </Typography>
      <List>
        {learningItems.map((item) => (
          <ListItem key={item.text} disablePadding>
            <ListItemButton onClick={() => handleNavigate(item.path)}>
              <ListItemIcon>{item.icon}</ListItemIcon>
              <ListItemText primary={item.text} />
            </ListItemButton>
          </ListItem>
        ))}
      </List>
    </div>
  );

  return (
    <Box sx={{ display: 'flex' }}>
      <AppBar
        position="fixed"
        sx={{
          width: { sm: `calc(100% - ${drawerWidth}px)` },
          ml: { sm: `${drawerWidth}px` },
        }}
      >
        <Toolbar>
          <IconButton
            color="inherit"
            edge="start"
            onClick={handleDrawerToggle}
            sx={{ mr: 2, display: { sm: 'none' } }}
          >
            <MenuIcon />
          </IconButton>
          <Typography variant="h6" noWrap component="div" sx={{ flexGrow: 1 }}>
            题库管理后台
          </Typography>
          <IconButton
            onClick={handleMenuOpen}
            size="small"
            sx={{ ml: 2 }}
          >
            <Avatar sx={{ width: 32, height: 32 }}>
              {user?.fullName?.charAt(0) || 'U'}
            </Avatar>
          </IconButton>
          <Menu
            anchorEl={anchorEl}
            open={Boolean(anchorEl)}
            onClose={handleMenuClose}
          >
            <MenuItem disabled>
              <Typography variant="body2">
                {user?.fullName} ({user?.role === UserRole.Admin ? '管理员' : user?.role === UserRole.Teacher ? '教师' : '学生'})
              </Typography>
            </MenuItem>
            <Divider />
            <MenuItem onClick={() => {
              handleMenuClose();
              navigate('/settings');
            }}>
              <ListItemIcon>
                <Settings fontSize="small" />
              </ListItemIcon>
              设置
            </MenuItem>
            <MenuItem onClick={handleLogout}>
              <ListItemIcon>
                <Logout fontSize="small" />
              </ListItemIcon>
              退出登录
            </MenuItem>
          </Menu>
        </Toolbar>
      </AppBar>
      <Box
        component="nav"
        sx={{ width: { sm: drawerWidth }, flexShrink: { sm: 0 } }}
      >
        <Drawer
          variant="temporary"
          open={mobileOpen}
          onClose={handleDrawerToggle}
          ModalProps={{
            keepMounted: true,
          }}
          sx={{
            display: { xs: 'block', sm: 'none' },
            '& .MuiDrawer-paper': { boxSizing: 'border-box', width: drawerWidth },
          }}
        >
          {drawer}
        </Drawer>
        <Drawer
          variant="permanent"
          sx={{
            display: { xs: 'none', sm: 'block' },
            '& .MuiDrawer-paper': { boxSizing: 'border-box', width: drawerWidth },
          }}
          open
        >
          {drawer}
        </Drawer>
      </Box>
      <Box
        component="main"
        sx={{
          flexGrow: 1,
          p: 3,
          width: { sm: `calc(100% - ${drawerWidth}px)` },
          mt: 8
        }}
      >
        <Outlet />
      </Box>
    </Box>
  );
};

export default DashboardLayout;
