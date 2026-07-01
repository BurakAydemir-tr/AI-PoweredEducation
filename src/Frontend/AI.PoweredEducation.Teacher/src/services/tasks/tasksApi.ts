import { apiClient } from '../api/apiClient'
import type {
  CreateGpsTaskRequest,
  CreateQrCodeTaskRequest,
  CreateQuizTaskRequest,
  LearningTask,
} from '../../types/api'

export async function createQuizTask(
  gameId: string,
  request: CreateQuizTaskRequest,
): Promise<LearningTask> {
  const response = await apiClient.post<LearningTask>(
    `/api/games/${gameId}/tasks/quiz`,
    request,
  )

  return response.data
}

export async function createQrTask(
  gameId: string,
  request: CreateQrCodeTaskRequest,
): Promise<LearningTask> {
  const response = await apiClient.post<LearningTask>(
    `/api/games/${gameId}/tasks/qr`,
    request,
  )

  return response.data
}

export async function createGpsTask(
  gameId: string,
  request: CreateGpsTaskRequest,
): Promise<LearningTask> {
  const response = await apiClient.post<LearningTask>(
    `/api/games/${gameId}/tasks/gps`,
    request,
  )

  return response.data
}

export async function updateQuizTask(
  taskId: string,
  request: CreateQuizTaskRequest,
): Promise<LearningTask> {
  const response = await apiClient.put<LearningTask>(
    `/api/tasks/${taskId}/quiz`,
    request,
  )

  return response.data
}

export async function updateQrTask(
  taskId: string,
  request: CreateQrCodeTaskRequest,
): Promise<LearningTask> {
  const response = await apiClient.put<LearningTask>(
    `/api/tasks/${taskId}/qr`,
    request,
  )

  return response.data
}

export async function updateGpsTask(
  taskId: string,
  request: CreateGpsTaskRequest,
): Promise<LearningTask> {
  const response = await apiClient.put<LearningTask>(
    `/api/tasks/${taskId}/gps`,
    request,
  )

  return response.data
}

export async function deleteTask(taskId: string): Promise<void> {
  await apiClient.delete(`/api/tasks/${taskId}`)
}

export async function reorderTasks(
  gameId: string,
  taskIds: string[],
): Promise<LearningTask[]> {
  const response = await apiClient.put<LearningTask[]>(
    `/api/games/${gameId}/tasks/order`,
    { taskIds },
  )

  return response.data
}
