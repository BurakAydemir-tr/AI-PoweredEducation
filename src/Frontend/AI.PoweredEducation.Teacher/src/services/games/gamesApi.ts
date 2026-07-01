import { apiClient } from '../api/apiClient'
import type { LearningGame, LearningGameRequest } from '../../types/api'

export async function getGames(): Promise<LearningGame[]> {
  const response = await apiClient.get<LearningGame[]>('/api/games')

  return response.data
}

export async function getGame(gameId: string): Promise<LearningGame> {
  const response = await apiClient.get<LearningGame>(`/api/games/${gameId}`)

  return response.data
}

export async function createGame(
  request: LearningGameRequest,
): Promise<LearningGame> {
  const response = await apiClient.post<LearningGame>('/api/games', request)

  return response.data
}

export async function updateGame(
  gameId: string,
  request: LearningGameRequest,
): Promise<LearningGame> {
  const response = await apiClient.put<LearningGame>(
    `/api/games/${gameId}`,
    request,
  )

  return response.data
}

export async function publishGame(gameId: string): Promise<LearningGame> {
  const response = await apiClient.post<LearningGame>(
    `/api/games/${gameId}/publish`,
  )

  return response.data
}

export async function activateGame(gameId: string): Promise<LearningGame> {
  const response = await apiClient.post<LearningGame>(
    `/api/games/${gameId}/activate`,
  )

  return response.data
}

export async function deactivateGame(gameId: string): Promise<LearningGame> {
  const response = await apiClient.post<LearningGame>(
    `/api/games/${gameId}/deactivate`,
  )

  return response.data
}

export async function archiveGame(gameId: string): Promise<LearningGame> {
  const response = await apiClient.post<LearningGame>(
    `/api/games/${gameId}/archive`,
  )

  return response.data
}

export async function restoreGame(gameId: string): Promise<LearningGame> {
  const response = await apiClient.post<LearningGame>(
    `/api/games/${gameId}/restore`,
  )

  return response.data
}

export async function cloneGame(gameId: string): Promise<LearningGame> {
  const response = await apiClient.post<LearningGame>(
    `/api/games/${gameId}/clone`,
  )

  return response.data
}
