import { apiClient } from '../api/apiClient'

export type LoginRequest = {
  email: string
  password: string
}

export type RegisterRequest = LoginRequest & {
  firstName: string
  lastName: string
}

export type AuthResponse = {
  accessToken: string
  accessTokenExpiresAt: string
  refreshToken: string
  refreshTokenExpiresAt: string
  firstName: string
  lastName: string
}

export async function login(request: LoginRequest): Promise<AuthResponse> {
  const response = await apiClient.post<AuthResponse>('/api/auth/login', request)

  return response.data
}

export async function register(request: RegisterRequest): Promise<AuthResponse> {
  const response = await apiClient.post<AuthResponse>('/api/auth/register', request)

  return response.data
}

export async function refresh(refreshToken: string): Promise<AuthResponse> {
  const response = await apiClient.post<AuthResponse>('/api/auth/refresh', {
    refreshToken,
  })

  return response.data
}
