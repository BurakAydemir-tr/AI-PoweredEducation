import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { Link as RouterLink, useNavigate } from 'react-router-dom'
import {
  Alert,
  Box,
  Button,
  Link,
  Stack,
  TextField,
  Typography,
} from '@mui/material'
import { register } from '../../services/auth/authApi'
import { tokenStorage } from '../../services/auth/tokenStorage'
import { getApiErrorMessage } from '../../utils/apiError'

type RegisterFormValues = {
  firstName: string
  lastName: string
  email: string
  password: string
}

export function RegisterPage() {
  const navigate = useNavigate()
  const [errorMessage, setErrorMessage] = useState<string | null>(null)

  const {
    formState: { errors, isSubmitting },
    handleSubmit,
    register: registerField,
  } = useForm<RegisterFormValues>({
    defaultValues: {
      firstName: '',
      lastName: '',
      email: '',
      password: '',
    },
  })

  const onSubmit = handleSubmit(async (values) => {
    setErrorMessage(null)

    try {
      const response = await register(values)

      tokenStorage.setAccessToken(response.accessToken)
      tokenStorage.setRefreshToken(response.refreshToken)
      tokenStorage.setTeacherName(response.firstName, response.lastName)

      navigate('/games', { replace: true })
    } catch (error) {
      setErrorMessage(getApiErrorMessage(error))
    }
  })

  return (
    <Stack spacing={4}>
      <Box>
        <Typography component="h1" variant="h4" gutterBottom>
          Öğretmen Hesabı Oluştur
        </Typography>
        <Typography color="text.secondary">
          Öğrenme oyunları oluşturmak ve yönetmek için öğretmen hesabı oluşturun.
        </Typography>
      </Box>

      <Box component="form" noValidate onSubmit={onSubmit}>
        <Stack spacing={3}>
          {errorMessage && <Alert severity="error">{errorMessage}</Alert>}

          <TextField
            autoComplete="given-name"
            autoFocus
            disabled={isSubmitting}
            error={Boolean(errors.firstName)}
            fullWidth
            helperText={errors.firstName?.message}
            label="Ad"
            {...registerField('firstName', {
              required: 'Ad zorunludur.',
            })}
          />

          <TextField
            autoComplete="family-name"
            disabled={isSubmitting}
            error={Boolean(errors.lastName)}
            fullWidth
            helperText={errors.lastName?.message}
            label="Soyad"
            {...registerField('lastName', {
              required: 'Soyad zorunludur.',
            })}
          />

          <TextField
            autoComplete="email"
            disabled={isSubmitting}
            error={Boolean(errors.email)}
            fullWidth
            helperText={errors.email?.message}
            label="E-posta"
            type="email"
            {...registerField('email', {
              required: 'E-posta zorunludur.',
            })}
          />

          <TextField
            autoComplete="new-password"
            disabled={isSubmitting}
            error={Boolean(errors.password)}
            fullWidth
            helperText={errors.password?.message}
            label="Şifre"
            type="password"
            {...registerField('password', {
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
            {isSubmitting ? 'Hesap oluşturuluyor...' : 'Hesap oluştur'}
          </Button>

          <Typography align="center" color="text.secondary" variant="body2">
            Zaten hesabınız var mı?{' '}
            <Link component={RouterLink} to="/login">
              Giriş yap
            </Link>
          </Typography>
        </Stack>
      </Box>
    </Stack>
  )
}
