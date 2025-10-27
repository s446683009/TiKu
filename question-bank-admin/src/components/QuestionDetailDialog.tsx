import React from 'react';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  Box,
  Typography,
  Chip,
  Divider,
  Stack
} from '@mui/material';
import type { Question } from '../types';
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

interface QuestionDetailDialogProps {
  open: boolean;
  question: Question | null;
  onClose: () => void;
}

const QuestionDetailDialog: React.FC<QuestionDetailDialogProps> = ({
  open,
  question,
  onClose
}) => {
  if (!question) return null;

  const parseOptions = (optionsStr?: string): string[] => {
    if (!optionsStr) return [];
    try {
      return JSON.parse(optionsStr);
    } catch {
      return [];
    }
  };

  const options = parseOptions(question.options);

  return (
    <Dialog
      open={open}
      onClose={onClose}
      maxWidth="md"
      fullWidth
      aria-labelledby="question-detail-dialog-title"
    >
      <DialogTitle id="question-detail-dialog-title">
        题目详情
      </DialogTitle>
      <DialogContent dividers>
        <Stack spacing={3}>
          {/* 基本信息 */}
          <Box>
            <Stack direction="row" spacing={1} mb={2}>
              <Chip label={QuestionTypeName[question.type]} size="small" />
              <Chip
                label={DifficultyName[question.difficulty]}
                size="small"
                color={DifficultyColor[question.difficulty] as any}
              />
              <Chip label={`${question.score}分`} size="small" color="primary" />
              {question.chapter && <Chip label={question.chapter} size="small" variant="outlined" />}
            </Stack>
          </Box>

          {/* 题干 */}
          <Box>
            <Typography variant="subtitle2" color="text.secondary" gutterBottom>
              题干
            </Typography>
            <Typography variant="body1" sx={{ whiteSpace: 'pre-wrap' }}>
              {question.content}
            </Typography>
          </Box>

          <Divider />

          {/* 选项 */}
          {options.length > 0 && (
            <Box>
              <Typography variant="subtitle2" color="text.secondary" gutterBottom>
                选项
              </Typography>
              <Stack spacing={1}>
                {options.map((option, index) => (
                  <Box key={index} sx={{ display: 'flex', alignItems: 'flex-start' }}>
                    <Typography variant="body2" sx={{ mr: 1, fontWeight: 'medium' }}>
                      {String.fromCharCode(65 + index)}.
                    </Typography>
                    <Typography variant="body2">{option}</Typography>
                  </Box>
                ))}
              </Stack>
            </Box>
          )}

          {options.length > 0 && <Divider />}

          {/* 正确答案 */}
          <Box>
            <Typography variant="subtitle2" color="text.secondary" gutterBottom>
              正确答案
            </Typography>
            <Typography
              variant="body1"
              sx={{
                color: 'success.main',
                fontWeight: 'medium',
                bgcolor: 'success.lighter',
                p: 1,
                borderRadius: 1
              }}
            >
              {question.correctAnswer}
            </Typography>
          </Box>

          {/* 解析 */}
          {question.explanation && (
            <>
              <Divider />
              <Box>
                <Typography variant="subtitle2" color="text.secondary" gutterBottom>
                  解析
                </Typography>
                <Typography variant="body2" sx={{ whiteSpace: 'pre-wrap' }}>
                  {question.explanation}
                </Typography>
              </Box>
            </>
          )}

          {/* 知识点 */}
          {question.knowledgePoints && question.knowledgePoints.length > 0 && (
            <>
              <Divider />
              <Box>
                <Typography variant="subtitle2" color="text.secondary" gutterBottom>
                  关联知识点
                </Typography>
                <Stack direction="row" spacing={1} flexWrap="wrap">
                  {question.knowledgePoints.map((kp) => (
                    <Chip key={kp.id} label={kp.name} size="small" variant="outlined" />
                  ))}
                </Stack>
              </Box>
            </>
          )}
        </Stack>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>关闭</Button>
      </DialogActions>
    </Dialog>
  );
};

export default QuestionDetailDialog;
