import {
  AppBar,
  Box,
  Button,
  Container,
  Divider,
  Stack,
  Toolbar,
  Typography,
} from '@mui/material'
import { Link as RouterLink, Outlet, useNavigate } from 'react-router-dom'
import { tokenStorage } from '../services/auth/tokenStorage'

export function TeacherLayout() {
  const navigate = useNavigate()
  const firstName = tokenStorage.getFirstName()
  const lastName = tokenStorage.getLastName()
  const teacherName = [firstName, lastName].filter(Boolean).join(' ')

  const handleLogout = () => {
    tokenStorage.clear()
    navigate('/login', { replace: true })
  }

  return (
    <Box sx={{ minHeight: '100vh', bgcolor: 'background.default' }}>
      <AppBar position="static" elevation={0}>
        <Toolbar>
          <Typography component="div" variant="h6" sx={{ flexGrow: 1 }}>
            AI Powered Education
          </Typography>
          <Stack direction="row" spacing={1} sx={{ alignItems: 'center' }}>
            <Button color="inherit" component={RouterLink} to="/games">
              Oyunlar
            </Button>
            {teacherName && (
              <>
                <Divider
                  flexItem
                  orientation="vertical"
                  sx={{ borderColor: 'rgba(255,255,255,0.35)' }}
                />
                <Typography color="inherit" variant="body2">
                  {teacherName}
                </Typography>
              </>
            )}
            <Button color="inherit" onClick={handleLogout}>
              Çıkış
            </Button>
          </Stack>
        </Toolbar>
      </AppBar>

      <Container component="main" maxWidth="lg" sx={{ py: 4 }}>
        <Outlet />
      </Container>
    </Box>
  )
}
