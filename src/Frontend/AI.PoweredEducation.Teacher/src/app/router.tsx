import { Navigate, createBrowserRouter } from 'react-router-dom'
import { AuthLayout } from '../layouts/AuthLayout'
import { TeacherLayout } from '../layouts/TeacherLayout'
import { LoginPage } from '../pages/auth/LoginPage'
import { RegisterPage } from '../pages/auth/RegisterPage'
import { DashboardPage } from '../pages/dashboard/DashboardPage'
import { GameEditorPage } from '../pages/games/GameEditorPage'
import { GamesPage } from '../pages/games/GamesPage'
import { ResultsPage } from '../pages/results/ResultsPage'
import { StudentPreviewPage } from '../pages/preview/StudentPreviewPage'
import { ProtectedRoute } from './ProtectedRoute'

export const router = createBrowserRouter([
  {
    path: '/',
    element: <Navigate to="/games" replace />,
  },
  {
    element: <AuthLayout />,
    children: [
      {
        path: '/login',
        element: <LoginPage />,
      },
      {
        path: '/register',
        element: <RegisterPage />,
      },
    ],
  },
  {
    element: (
      <ProtectedRoute>
        <TeacherLayout />
      </ProtectedRoute>
    ),
    children: [
      {
        path: '/dashboard',
        element: <DashboardPage />,
      },
      {
        path: '/games',
        element: <GamesPage />,
      },
      {
        path: '/games/:gameId',
        element: <GameEditorPage />,
      },
      {
        path: '/games/:gameId/preview',
        element: <StudentPreviewPage />,
      },
      {
        path: '/games/:gameId/results',
        element: <ResultsPage />,
      },
    ],
  },
])
