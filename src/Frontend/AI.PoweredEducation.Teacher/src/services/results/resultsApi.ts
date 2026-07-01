import { apiClient } from '../api/apiClient'
import type { GameResults } from '../../types/api'

export async function getGameResults(gameId: string): Promise<GameResults> {
  const response = await apiClient.get<GameResults>(
    `/api/games/${gameId}/results`,
  )

  return response.data
}
