import React, { useState, useEffect } from 'react';
import {
  Box,
  Paper,
  Typography,
  Button,
  List,
  ListItem,
  ListItemText,
  IconButton,
  Chip,
  Stack,
  Collapse
} from '@mui/material';
import {
  Add,
  Edit,
  Delete,
  ExpandMore,
  ChevronRight,
  FolderOpen,
  Folder
} from '@mui/icons-material';
import apiService from '../api/apiService';
import type { KnowledgePoint } from '../types';

const KnowledgePointsPage: React.FC = () => {
  const [knowledgePoints, setKnowledgePoints] = useState<KnowledgePoint[]>([]);
  const [expandedIds, setExpandedIds] = useState<Set<string>>(new Set());

  useEffect(() => {
    loadKnowledgePoints();
  }, []);

  const loadKnowledgePoints = async () => {
    try {
      const response = await apiService.getKnowledgePoints();

      if (response.success && response.data) {
        const points = Array.isArray(response.data) ? response.data : [];
        setKnowledgePoints(points);
      }
    } catch (error) {
      console.error('Failed to load knowledge points:', error);
    }
  };

  const toggleExpand = (id: string) => {
    const newExpanded = new Set(expandedIds);
    if (newExpanded.has(id)) {
      newExpanded.delete(id);
    } else {
      newExpanded.add(id);
    }
    setExpandedIds(newExpanded);
  };

  // 构建树形结构
  const buildTree = (points: KnowledgePoint[]): KnowledgePoint[] => {
    return points.filter(p => !p.parentId);
  };

  const getChildren = (parentId: string): KnowledgePoint[] => {
    return knowledgePoints.filter(p => p.parentId === parentId);
  };

  const renderKnowledgePoint = (point: KnowledgePoint, level: number = 0): React.ReactNode => {
    const children = getChildren(point.id);
    const hasChildren = children.length > 0;
    const isExpanded = expandedIds.has(point.id);

    return (
      <Box key={point.id}>
        <ListItem
          sx={{
            pl: 2 + level * 3,
            '&:hover': {
              bgcolor: 'action.hover'
            }
          }}
        >
          {hasChildren ? (
            <IconButton
              size="small"
              onClick={() => toggleExpand(point.id)}
              sx={{ mr: 1 }}
            >
              {isExpanded ? <ExpandMore /> : <ChevronRight />}
            </IconButton>
          ) : (
            <Box sx={{ width: 40 }} />
          )}

          {hasChildren ? (
            isExpanded ? (
              <FolderOpen sx={{ mr: 1, color: 'primary.main' }} />
            ) : (
              <Folder sx={{ mr: 1, color: 'action.active' }} />
            )
          ) : (
            <Box sx={{ width: 24, mr: 1 }} />
          )}

          <ListItemText
            primary={
              <Stack direction="row" spacing={1} alignItems="center">
                <Typography variant="body1">{point.name}</Typography>
                <Chip label={`Level ${point.level}`} size="small" />
              </Stack>
            }
            secondary={point.description}
          />

          <IconButton size="small" color="primary" title="编辑">
            <Edit fontSize="small" />
          </IconButton>
          <IconButton size="small" color="primary" title="添加子节点">
            <Add fontSize="small" />
          </IconButton>
          <IconButton size="small" color="error" title="删除">
            <Delete fontSize="small" />
          </IconButton>
        </ListItem>

        {hasChildren && (
          <Collapse in={isExpanded} timeout="auto" unmountOnExit>
            {children.map(child => renderKnowledgePoint(child, level + 1))}
          </Collapse>
        )}
      </Box>
    );
  };

  const rootPoints = buildTree(knowledgePoints);

  return (
    <Box>
      <Box display="flex" justifyContent="space-between" alignItems="center" mb={3}>
        <Typography variant="h4">知识点管理</Typography>
        <Stack direction="row" spacing={2}>
          <Button variant="outlined" onClick={() => setExpandedIds(new Set(knowledgePoints.map(p => p.id)))}>
            展开全部
          </Button>
          <Button variant="outlined" onClick={() => setExpandedIds(new Set())}>
            折叠全部
          </Button>
          <Button variant="contained" startIcon={<Add />}>
            添加根节点
          </Button>
        </Stack>
      </Box>

      <Paper>
        {rootPoints.length === 0 ? (
          <Box p={3} textAlign="center">
            <Typography variant="body2" color="text.secondary">
              暂无知识点数据，请添加根知识点
            </Typography>
          </Box>
        ) : (
          <List sx={{ p: 0 }}>
            {rootPoints.map(point => renderKnowledgePoint(point))}
          </List>
        )}
      </Paper>
    </Box>
  );
};

export default KnowledgePointsPage;
