import { apiClient } from '../api/apiClient'
import type {
  AiGenerationRequest,
  GeneratedQrCodeTask,
  GeneratedQuizTask,
} from '../../types/api'

export async function generateQuizTasks(
  request: AiGenerationRequest,
): Promise<GeneratedQuizTask[]> {
  const response = await apiClient.post<GeneratedQuizTask[]>(
    '/api/ai/quiz-tasks',
    request,
  )

  return response.data
}

export async function generateQrCodeTasks(
  request: AiGenerationRequest,
): Promise<GeneratedQrCodeTask[]> {
  const response = await apiClient.post<GeneratedQrCodeTask[]>(
    '/api/ai/qr-code-tasks',
    request,
  )

  return response.data
}
