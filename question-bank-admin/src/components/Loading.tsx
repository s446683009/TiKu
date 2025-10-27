import React from 'react';
import { Box, CircularProgress, Typography } from '@mui/material';

interface LoadingProps {
  message?: string;
  fullScreen?: boolean;
}

const Loading: React.FC<LoadingProps> = ({ message = '加载中...', fullScreen = false }) => {
  if (fullScreen) {
    return (
      <Box
        sx={{
          position: 'fixed',
          top: 0,
          left: 0,
          right: 0,
          bottom: 0,
          display: 'flex',
          flexDirection: 'column',
          alignItems: 'center',
          justifyContent: 'center',
          bgcolor: 'rgba(255, 255, 255, 0.9)',
          zIndex: 9999
        }}
      >
        <CircularProgress size={60} />
        <Typography variant="body1" sx={{ mt: 2 }} color="text.secondary">
          {message}
        </Typography>
      </Box>
    );
  }

  return (
    <Box
      sx={{
        display: 'flex',
        flexDirection: 'column',
        alignItems: 'center',
        justifyContent: 'center',
        py: 8
      }}
    >
      <CircularProgress />
      <Typography variant="body2" sx={{ mt: 2 }} color="text.secondary">
        {message}
      </Typography>
    </Box>
  );
};

export default Loading;
