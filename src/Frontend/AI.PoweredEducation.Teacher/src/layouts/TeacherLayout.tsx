import {
  AppBar,
  Box,
  Button,
  Container,
  Divider,
  Stack,
  Toolbar,
} from '@mui/material'
import { Link as RouterLink, Outlet, useNavigate } from 'react-router-dom'
import { BrandLogo } from '../components/BrandLogo'
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
          <Box component={RouterLink} to="/dashboard" sx={{ flexGrow: 1, textDecoration: 'none' }}>
            <BrandLogo compact inverted />
          </Box>
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
                <Box component="span" sx={{ color: 'inherit', fontSize: 14 }}>
                  {teacherName}
                </Box>
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
