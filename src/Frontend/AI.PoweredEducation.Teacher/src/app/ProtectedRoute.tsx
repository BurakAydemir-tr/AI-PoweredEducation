import { Navigate, useLocation } from 'react-router-dom'
import type { ReactNode } from 'react'
import { tokenStorage } from '../services/auth/tokenStorage'

type ProtectedRouteProps = {
  children: ReactNode
}

export function ProtectedRoute({ children }: ProtectedRouteProps) {
  const location = useLocation()

  if (!tokenStorage.getAccessToken()) {
    return <Navigate to="/login" replace state={{ from: location }} />
  }

  return children
}
