import { useEffect, useState } from 'react'
import { Link as RouterLink, useNavigate } from 'react-router-dom'
import {
  Alert,
  Box,
  Button,
  Card,
  CardActions,
  CardContent,
  Dialog,
  DialogContent,
  DialogTitle,
  Grid,
  Stack,
  Typography,
} from '@mui/material'
import { GameForm } from '../../components/GameForm'
import { ErrorState, LoadingState } from '../../components/PageState'
import { StatusChip } from '../../components/StatusChip'
import type { LearningGame, LearningGameRequest } from '../../types/api'
import {
  activateGame,
  archiveGame,
  cloneGame,
  createGame,
  deactivateGame,
  getGames,
  restoreGame,
} from '../../services/games/gamesApi'
import { gameEnvironmentTypeLabels } from '../../utils/enumLabels'
import { getApiErrorMessage } from '../../utils/apiError'

export function GamesPage() {
  const navigate = useNavigate()
  const [games, setGames] = useState<LearningGame[]>([])
  const [isCreateOpen, setIsCreateOpen] = useState(false)
  const [isLoading, setIsLoading] = useState(true)
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [errorMessage, setErrorMessage] = useState<string | null>(null)

  const loadGames = async () => {
    setIsLoading(true)
    setErrorMessage(null)

    try {
      setGames(await getGames())
    } catch (error) {
      setErrorMessage(getApiErrorMessage(error))
    } finally {
      setIsLoading(false)
    }
  }

  useEffect(() => {
    void loadGames()
  }, [])

  const handleCreate = async (values: LearningGameRequest) => {
    setIsSubmitting(true)
    setErrorMessage(null)

    try {
      const created = await createGame(values)
      setIsCreateOpen(false)
      navigate(`/games/${created.id}`)
    } catch (error) {
      setErrorMessage(getApiErrorMessage(error))
    } finally {
      setIsSubmitting(false)
    }
  }

  const runGameAction = async (
    action: (gameId: string) => Promise<LearningGame>,
    gameId: string,
  ) => {
    setErrorMessage(null)

    try {
      const updated = await action(gameId)
      setGames((current) =>
        current.map((game) => (game.id === updated.id ? updated : game)),
      )
    } catch (error) {
      setErrorMessage(getApiErrorMessage(error))
    }
  }

  const handleClone = async (gameId: string) => {
    setErrorMessage(null)

    try {
      const cloned = await cloneGame(gameId)
      setGames((current) => [cloned, ...current])
    } catch (error) {
      setErrorMessage(getApiErrorMessage(error))
    }
  }

  if (isLoading) {
    return <LoadingState message="Oyunlar yükleniyor..." />
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
            Öğrenme Oyunları
          </Typography>
          <Typography color="text.secondary">
            Öğretmen kontrolünde oyunlar oluşturun, düzenleyin, yayınlayın ve aktifleştirin.
          </Typography>
        </Box>
        <Button onClick={() => setIsCreateOpen(true)} variant="contained">
          Oyun oluştur
        </Button>
      </Stack>

      {errorMessage && <Alert severity="error">{errorMessage}</Alert>}

      {games.length === 0 ? (
        <ErrorState message="Henüz oyun yok. İlk öğrenme oyununuzu oluşturun." />
      ) : (
        <Grid container spacing={3}>
          {games.map((game) => (
            <Grid key={game.id} size={{ xs: 12, md: 6, lg: 4 }}>
              <Card sx={{ height: '100%' }}>
                <CardContent>
                  <Stack spacing={1.5}>
                    <Stack
                      direction="row"
                      sx={{
                        alignItems: 'center',
                        justifyContent: 'space-between',
                      }}
                    >
                      <StatusChip status={game.status} />
                      <Typography color="text.secondary" variant="caption">
                        Kod: {game.gameCode}
                      </Typography>
                    </Stack>
                    <Typography component="h2" variant="h6">
                      {game.subject} / {game.topic}
                    </Typography>
                    <Typography color="text.secondary">
                      Sınıf {game.gradeLevel} -{' '}
                      {gameEnvironmentTypeLabels[game.environmentType]} -{' '}
                      {game.expectedStudentCount} beklenen öğrenci
                    </Typography>
                    <Typography color="text.secondary" variant="body2">
                      {game.tasks.length} görev
                    </Typography>
                  </Stack>
                </CardContent>
                <CardActions sx={{ flexWrap: 'wrap', gap: 1 }}>
                  <Button component={RouterLink} size="small" to={`/games/${game.id}`}>
                    Düzenle
                  </Button>
                  <Button
                    component={RouterLink}
                    size="small"
                    to={`/games/${game.id}/results`}
                  >
                    Sonuçlar
                  </Button>
                  <Button size="small" onClick={() => void handleClone(game.id)}>
                    Kopyala
                  </Button>
                  {(game.status === 0 || game.status === 1) && (
                    <Button
                      size="small"
                      onClick={() => void runGameAction(activateGame, game.id)}
                    >
                      Aktifleştir
                    </Button>
                  )}
                  {game.status === 2 && (
                    <Button
                      size="small"
                      onClick={() => void runGameAction(deactivateGame, game.id)}
                    >
                      Pasifleştir
                    </Button>
                  )}
                  {game.status !== 0 && game.status !== 3 && (
                    <Button
                      color="warning"
                      size="small"
                      onClick={() => void runGameAction(archiveGame, game.id)}
                    >
                      Arşivle
                    </Button>
                  )}
                  {game.status === 3 && (
                    <Button
                      size="small"
                      onClick={() => void runGameAction(restoreGame, game.id)}
                    >
                      Geri yükle
                    </Button>
                  )}
                </CardActions>
              </Card>
            </Grid>
          ))}
        </Grid>
      )}

      <Dialog
        fullWidth
        maxWidth="sm"
        open={isCreateOpen}
        onClose={() => setIsCreateOpen(false)}
      >
        <DialogTitle>Öğrenme oyunu oluştur</DialogTitle>
        <DialogContent>
          <GameForm
            isSubmitting={isSubmitting}
            submitLabel="Oyun oluştur"
            onSubmit={handleCreate}
          />
        </DialogContent>
      </Dialog>
    </Stack>
  )
}
