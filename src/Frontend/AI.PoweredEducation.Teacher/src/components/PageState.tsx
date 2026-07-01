import { Alert, Box, CircularProgress } from '@mui/material'

type LoadingStateProps = {
  message?: string
}

export function LoadingState({ message = 'Yükleniyor...' }: LoadingStateProps) {
  return (
    <Box sx={{ alignItems: 'center', display: 'flex', gap: 2, py: 4 }}>
      <CircularProgress size={24} />
      <span>{message}</span>
    </Box>
  )
}

type ErrorStateProps = {
  message: string
}

export function ErrorState({ message }: ErrorStateProps) {
  return <Alert severity="error">{message}</Alert>
}
