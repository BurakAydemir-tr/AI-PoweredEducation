import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { useNavigate } from 'react-router-dom'
import {
  Alert,
  Box,
  Button,
  Link,
  Stack,
  TextField,
  Typography,
} from '@mui/material'
import { Link as RouterLink } from 'react-router-dom'
import { login } from '../../services/auth/authApi'
import { tokenStorage } from '../../services/auth/tokenStorage'
import { getApiErrorMessage } from '../../utils/apiError'

type LoginFormValues = {
  email: string
  password: string
}

export function LoginPage() {
  const navigate = useNavigate()
  const [errorMessage, setErrorMessage] = useState<string | null>(null)

  const {
    formState: { errors, isSubmitting },
    handleSubmit,
    register,
  } = useForm<LoginFormValues>({
    defaultValues: {
      email: '',
      password: '',
    },
  })

  const onSubmit = handleSubmit(async (values) => {
    setErrorMessage(null)

    try {
      const response = await login(values)

      tokenStorage.setAccessToken(response.accessToken)
      tokenStorage.setRefreshToken(response.refreshToken)
      tokenStorage.setTeacherName(response.firstName, response.lastName)

      navigate('/dashboard', { replace: true })
    } catch (error) {
      setErrorMessage(getApiErrorMessage(error))
    }
  })

  return (
    <Stack spacing={4}>
      <Box>
        <Typography component="h1" variant="h4" gutterBottom>
          Öğretmen Girişi
        </Typography>
        <Typography color="text.secondary">
          Öğrenme oyunlarını yönetmek ve öğrenci sonuçlarını takip etmek için giriş yapın.
        </Typography>
      </Box>

      <Box component="form" noValidate onSubmit={onSubmit}>
        <Stack spacing={3}>
          {errorMessage && <Alert severity="error">{errorMessage}</Alert>}

          <TextField
            autoComplete="email"
            autoFocus
            disabled={isSubmitting}
            error={Boolean(errors.email)}
            fullWidth
            helperText={errors.email?.message}
            label="E-posta"
            type="email"
            {...register('email', {
              required: 'E-posta zorunludur.',
            })}
          />

          <TextField
            autoComplete="current-password"
            disabled={isSubmitting}
            error={Boolean(errors.password)}
            fullWidth
            helperText={errors.password?.message}
            label="Şifre"
            type="password"
            {...register('password', {
              required: 'Şifre zorunludur.',
            })}
          />

          <Button
            disabled={isSubmitting}
            fullWidth
            size="large"
            type="submit"
            variant="contained"
          >
            {isSubmitting ? 'Giriş yapılıyor...' : 'Giriş yap'}
          </Button>

          <Typography align="center" color="text.secondary" variant="body2">
            Hesabınız yok mu?{' '}
            <Link component={RouterLink} to="/register">
              Kayıt ol
            </Link>
          </Typography>
        </Stack>
      </Box>
    </Stack>
  )
}
