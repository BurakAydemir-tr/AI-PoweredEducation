import { Button, Paper, Stack, Typography } from '@mui/material'
import { Link as RouterLink } from 'react-router-dom'

export function DashboardPage() {
  return (
    <Paper sx={{ p: 4 }}>
      <Stack spacing={1}>
        <Typography component="h1" variant="h5">
          Öğretmen Paneli
        </Typography>
        <Typography color="text.secondary">
          Öğrenme oyunlarını yönetin, görev oluşturun, oyunları yayınlayın ve sonuçları inceleyin.
        </Typography>
        <Button component={RouterLink} sx={{ alignSelf: 'flex-start', mt: 2 }} to="/games" variant="contained">
          Oyunları aç
        </Button>
      </Stack>
    </Paper>
  )
}
