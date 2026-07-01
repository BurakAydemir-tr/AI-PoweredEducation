import { useEffect, useMemo, useState } from 'react'
import { Link as RouterLink, useParams } from 'react-router-dom'
import {
  Box,
  Button,
  Card,
  CardContent,
  LinearProgress,
  Stack,
  Typography,
} from '@mui/material'
import { ErrorState, LoadingState } from '../../components/PageState'
import type { LearningGame } from '../../types/api'
import { getGame } from '../../services/games/gamesApi'
import { getApiErrorMessage } from '../../utils/apiError'
import {
  learningTaskTypeLabels,
  quizAnswerOptionLabels,
} from '../../utils/enumLabels'

export function StudentPreviewPage() {
  const { gameId } = useParams()
  const [game, setGame] = useState<LearningGame | null>(null)
  const [activeIndex, setActiveIndex] = useState(0)
  const [isLoading, setIsLoading] = useState(true)
  const [errorMessage, setErrorMessage] = useState<string | null>(null)

  useEffect(() => {
    const loadGame = async () => {
      if (!gameId) {
        return
      }

      setIsLoading(true)

      try {
        setGame(await getGame(gameId))
      } catch (error) {
        setErrorMessage(getApiErrorMessage(error))
      } finally {
        setIsLoading(false)
      }
    }

    void loadGame()
  }, [gameId])

  const tasks = useMemo(
    () => [...(game?.tasks ?? [])].sort((a, b) => a.order - b.order),
    [game],
  )
  const activeTask = tasks[activeIndex]

  if (isLoading) {
    return <LoadingState message="Önizleme yükleniyor..." />
  }

  if (!game) {
    return <ErrorState message={errorMessage ?? 'Oyun bulunamadı.'} />
  }

  return (
    <Stack spacing={3}>
      <Stack
        direction={{ xs: 'column', sm: 'row' }}
        spacing={2}
        sx={{
          alignItems: { xs: 'stretch', sm: 'center' },
          justifyContent: 'space-between',
        }}
      >
        <Box>
          <Typography component="h1" variant="h4">
            Öğrenci Önizleme
          </Typography>
          <Typography color="text.secondary">
            Tek seferde bir görev önizlemesi: {game.subject} / {game.topic}.
          </Typography>
        </Box>
        <Button component={RouterLink} to={`/games/${game.id}`}>
          Editöre dön
        </Button>
      </Stack>

      {tasks.length === 0 ? (
        <ErrorState message="Önizlenecek görev yok." />
      ) : (
        <Card sx={{ maxWidth: 640 }}>
          <CardContent>
            <Stack spacing={3}>
              <Box>
                <Typography color="text.secondary">
                  Görev {activeIndex + 1}/{tasks.length} -{' '}
                  {learningTaskTypeLabels[activeTask.taskType]}
                </Typography>
                <LinearProgress
                  sx={{ mt: 1 }}
                  value={((activeIndex + 1) / tasks.length) * 100}
                  variant="determinate"
                />
              </Box>

              <Typography variant="h5">
                {activeTask.taskType === 0
                  ? activeTask.question
                  : activeTask.instructions}
              </Typography>

              {activeTask.taskType === 0 && (
                <Stack spacing={1}>
                  <Typography>A. {activeTask.optionA}</Typography>
                  <Typography>B. {activeTask.optionB}</Typography>
                  <Typography>C. {activeTask.optionC}</Typography>
                  <Typography>D. {activeTask.optionD}</Typography>
                  <Typography color="text.secondary">
                    Doğru cevap:{' '}
                    {activeTask.correctAnswer !== null &&
                    activeTask.correctAnswer !== undefined
                      ? quizAnswerOptionLabels[activeTask.correctAnswer]
                      : '-'}
                  </Typography>
                </Stack>
              )}

              {activeTask.taskType === 1 && (
                <Typography color="text.secondary">
                  QR içeriği: {activeTask.qrPayload}
                </Typography>
              )}

              {activeTask.taskType === 2 && (
                <Typography color="text.secondary">
                  Hedef: {activeTask.targetLatitude}, {activeTask.targetLongitude}
                </Typography>
              )}

              <Stack direction="row" sx={{ justifyContent: 'space-between' }}>
                <Button
                  disabled={activeIndex === 0}
                  onClick={() => setActiveIndex((current) => current - 1)}
                >
                  Önceki
                </Button>
                <Button
                  disabled={activeIndex === tasks.length - 1}
                  onClick={() => setActiveIndex((current) => current + 1)}
                  variant="contained"
                >
                  Sonraki
                </Button>
              </Stack>
            </Stack>
          </CardContent>
        </Card>
      )}
    </Stack>
  )
}




