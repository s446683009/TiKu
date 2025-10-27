import React from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router';
import { CssBaseline, ThemeProvider, createTheme } from '@mui/material';
import { AuthProvider, useAuth } from './contexts/AuthContext';
import ProtectedRoute from './components/ProtectedRoute';
import DashboardLayout from './layouts/DashboardLayout';
import LoginPage from './pages/LoginPage';
import DashboardPage from './pages/DashboardPage';
import QuestionsPage from './pages/QuestionsPage';
import PapersPage from './pages/PapersPage';
import ExamsPage from './pages/ExamsPage';
import KnowledgePointsPage from './pages/KnowledgePointsPage';
import UsersPage from './pages/UsersPage';
import FavoritesPage from './pages/FavoritesPage';
import NotesPage from './pages/NotesPage';
import WrongQuestionsPage from './pages/WrongQuestionsPage';
import SettingsPage from './pages/SettingsPage';
import { UserRole } from './types';

const theme = createTheme({
  palette: {
    primary: {
      main: '#1976d2',
    },
    secondary: {
      main: '#dc004e',
    },
  },
});

const AppRoutes: React.FC = () => {
  const { isAuthenticated } = useAuth();

  return (
    <Routes>
      <Route path="/login" element={
        isAuthenticated ? <Navigate to="/" replace /> : <LoginPage />
      } />

      <Route path="/" element={
        <ProtectedRoute>
          <DashboardLayout />
        </ProtectedRoute>
      }>
        <Route index element={<DashboardPage />} />
        <Route path="questions" element={
          <ProtectedRoute requiredRole={UserRole.Teacher}>
            <QuestionsPage />
          </ProtectedRoute>
        } />
        <Route path="papers" element={
          <ProtectedRoute requiredRole={UserRole.Teacher}>
            <PapersPage />
          </ProtectedRoute>
        } />
        <Route path="exams" element={
          <ProtectedRoute requiredRole={UserRole.Teacher}>
            <ExamsPage />
          </ProtectedRoute>
        } />
        <Route path="knowledge-points" element={
          <ProtectedRoute requiredRole={UserRole.Teacher}>
            <KnowledgePointsPage />
          </ProtectedRoute>
        } />
        <Route path="users" element={
          <ProtectedRoute requiredRole={UserRole.Admin}>
            <UsersPage />
          </ProtectedRoute>
        } />
        <Route path="favorites" element={<FavoritesPage />} />
        <Route path="notes" element={<NotesPage />} />
        <Route path="wrong-questions" element={<WrongQuestionsPage />} />
        <Route path="settings" element={<SettingsPage />} />
      </Route>

      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
};

function App() {
  return (
    <ThemeProvider theme={theme}>
      <CssBaseline />
      <BrowserRouter>
        <AuthProvider>
          <AppRoutes />
        </AuthProvider>
      </BrowserRouter>
    </ThemeProvider>
  );
}

export default App;
