import { useCallback, useEffect, useMemo, useState } from 'react'
import { Link as RouterLink, useParams } from 'react-router-dom'
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
  Divider,
  FormControl,
  InputLabel,
  MenuItem,
  Paper,
  Select,
  Stack,
  Tab,
  Tabs,
  TextField,
  Typography,
} from '@mui/material'
import { QRCodeCanvas } from 'qrcode.react'
import {
  CircleMarker,
  MapContainer,
  TileLayer,
  useMap,
  useMapEvents,
} from 'react-leaflet'
import { GameForm } from '../../components/GameForm'
import { LoadingState } from '../../components/PageState'
import { StatusChip } from '../../components/StatusChip'
import type {
  AiGenerationRequest,
  CreateGpsTaskRequest,
  CreateQrCodeTaskRequest,
  CreateQuizTaskRequest,
  GeneratedQrCodeTask,
  GeneratedQuizTask,
  LearningGame,
  LearningGameRequest,
  LearningTask,
  LearningTaskType,
  QuizAnswerOption,
} from '../../types/api'
import {
  activateGame,
  deactivateGame,
  getGame,
  updateGame,
} from '../../services/games/gamesApi'
import {
  createGpsTask,
  createQrTask,
  createQuizTask,
  deleteTask,
  reorderTasks,
  updateGpsTask,
  updateQrTask,
  updateQuizTask,
} from '../../services/tasks/tasksApi'
import {
  generateQrCodeTasks,
  generateQuizTasks,
} from '../../services/ai/aiApi'
import { getApiErrorMessage } from '../../utils/apiError'
import {
  defaultGameAreaJson,
  learningTaskTypeLabels,
  quizAnswerOptionLabels,
} from '../../utils/enumLabels'

type TaskFormState = {
  id?: string
  taskType: LearningTaskType
  question: string
  optionA: string
  optionB: string
  optionC: string
  optionD: string
  correctAnswer: QuizAnswerOption
  instructions: string
  qrPayload: string
  timeLimitMinutes: number
  targetLatitude: number
  targetLongitude: number
  gameAreaJson: string
}

const defaultTaskForm: TaskFormState = {
  taskType: 0,
  question: '',
  optionA: '',
  optionB: '',
  optionC: '',
  optionD: '',
  correctAnswer: 0,
  instructions: '',
  qrPayload: '',
  timeLimitMinutes: 3,
  targetLatitude: 39.9208,
  targetLongitude: 32.8541,
  gameAreaJson: defaultGameAreaJson,
}

function toTaskForm(task: LearningTask): TaskFormState {
  return {
    id: task.id,
    taskType: task.taskType,
    question: task.question ?? '',
    optionA: task.optionA ?? '',
    optionB: task.optionB ?? '',
    optionC: task.optionC ?? '',
    optionD: task.optionD ?? '',
    correctAnswer: task.correctAnswer ?? 0,
    instructions: task.instructions ?? '',
    qrPayload: task.qrPayload ?? '',
    timeLimitMinutes: task.timeLimitMinutes ?? 3,
    targetLatitude: task.targetLatitude ?? 39.9208,
    targetLongitude: task.targetLongitude ?? 32.8541,
    gameAreaJson: task.gameAreaJson ?? defaultGameAreaJson,
  }
}

function TaskLocationPicker({
  latitude,
  longitude,
  onChange,
}: {
  latitude: number
  longitude: number
  onChange: (latitude: number, longitude: number) => void
}) {
  useMapEvents({
    click(event) {
      onChange(event.latlng.lat, event.latlng.lng)
    },
  })

  return (
    <CircleMarker
      center={[latitude, longitude]}
      pathOptions={{ color: '#2563eb' }}
      radius={8}
    />
  )
}

function MapSearchControl({
  onLocationFound,
}: {
  onLocationFound: (latitude: number, longitude: number) => void
}) {
  const map = useMap()
  const [query, setQuery] = useState('')
  const [error, setError] = useState<string | null>(null)
  const [isSearching, setIsSearching] = useState(false)

  const searchLocation = async () => {
    const searchText = query.trim()

    if (!searchText) {
      setError('Aranacak konumu yazın.')
      return
    }

    setIsSearching(true)
    setError(null)

    try {
      const response = await fetch(
        `https://nominatim.openstreetmap.org/search?format=json&limit=1&q=${encodeURIComponent(searchText)}`,
      )
      const results = (await response.json()) as Array<{
        lat: string
        lon: string
      }>
      const firstResult = results[0]

      if (!firstResult) {
        setError('Konum bulunamadı.')
        return
      }

      const latitude = Number(firstResult.lat)
      const longitude = Number(firstResult.lon)

      map.setView([latitude, longitude], 13)
      onLocationFound(latitude, longitude)
    } catch {
      setError('Konum aranırken hata oluştu.')
    } finally {
      setIsSearching(false)
    }
  }

  return (
    <Box
      sx={{
        left: 12,
        position: 'absolute',
        right: 12,
        top: 12,
        zIndex: 1000,
      }}
    >
      <Stack
        direction={{ xs: 'column', sm: 'row' }}
        spacing={1}
        sx={{
          bgcolor: 'background.paper',
          borderRadius: 2,
          boxShadow: 2,
          p: 1,
        }}
      >
        <TextField
          fullWidth
          label="Konum ara"
          placeholder="Örn: Denizli"
          size="small"
          value={query}
          onChange={(event) => setQuery(event.target.value)}
          onKeyDown={(event) => {
            if (event.key === 'Enter') {
              event.preventDefault()
              void searchLocation()
            }
          }}
        />
        <Button
          disabled={isSearching}
          onClick={() => void searchLocation()}
          variant="contained"
        >
          Ara
        </Button>
      </Stack>
      {error && (
        <Alert severity="warning" sx={{ mt: 1 }}>
          {error}
        </Alert>
      )}
    </Box>
  )
}

export function GameEditorPage() {
  const { gameId } = useParams()
  const [game, setGame] = useState<LearningGame | null>(null)
  const [tab, setTab] = useState(0)
  const [isLoading, setIsLoading] = useState(true)
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [errorMessage, setErrorMessage] = useState<string | null>(null)
  const [successMessage, setSuccessMessage] = useState<string | null>(null)
  const [taskDialogOpen, setTaskDialogOpen] = useState(false)
  const [taskDialogError, setTaskDialogError] = useState<string | null>(null)
  const [taskForm, setTaskForm] = useState<TaskFormState>(defaultTaskForm)
  const [draggedTaskId, setDraggedTaskId] = useState<string | null>(null)
  const [aiTaskCount, setAiTaskCount] = useState(3)
  const [generatedQuizTasks, setGeneratedQuizTasks] = useState<GeneratedQuizTask[]>(
    [],
  )
  const [generatedQrTasks, setGeneratedQrTasks] = useState<GeneratedQrCodeTask[]>(
    [],
  )

  const orderedGörevler = useMemo(
    () => [...(game?.tasks ?? [])].sort((a, b) => a.order - b.order),
    [game],
  )

  const loadGame = useCallback(async () => {
    if (!gameId) {
      return
    }

    setIsLoading(true)
    setErrorMessage(null)

    try {
      setGame(await getGame(gameId))
    } catch (error) {
      setErrorMessage(getApiErrorMessage(error))
    } finally {
      setIsLoading(false)
    }
  }, [gameId])

  useEffect(() => {
    void loadGame()
  }, [loadGame])

  const refreshGame = async () => {
    if (!gameId) {
      return
    }

    setGame(await getGame(gameId))
  }

  const showSuccess = (message: string) => {
    setSuccessMessage(message)
    window.setTimeout(() => setSuccessMessage(null), 3000)
  }

  const handleUpdateGame = async (values: LearningGameRequest) => {
    if (!gameId) {
      return
    }

    setIsSubmitting(true)
    setErrorMessage(null)

    try {
      setGame(await updateGame(gameId, values))
      showSuccess('Oyun bilgileri kaydedildi.')
    } catch (error) {
      setErrorMessage(getApiErrorMessage(error))
    } finally {
      setIsSubmitting(false)
    }
  }

  const runLifecycleAction = async (
    action: (id: string) => Promise<LearningGame>,
    message: string,
  ) => {
    if (!gameId) {
      return
    }

    setErrorMessage(null)

    try {
      setGame(await action(gameId))
      showSuccess(message)
    } catch (error) {
      setErrorMessage(getApiErrorMessage(error))
    }
  }

  const openCreateTaskDialog = (taskType: LearningTaskType) => {
    setTaskForm({ ...defaultTaskForm, taskType })
    setTaskDialogError(null)
    setTaskDialogOpen(true)
  }

  const openEditTaskDialog = (task: LearningTask) => {
    setTaskForm(toTaskForm(task))
    setTaskDialogError(null)
    setTaskDialogOpen(true)
  }

  const handleTaskSubmit = async () => {
    if (!gameId) {
      return
    }

    setIsSubmitting(true)
    setTaskDialogError(null)

    if (taskForm.taskType === 1 && !taskForm.qrPayload.trim()) {
      setTaskDialogError('QR kelimesi zorunludur.')
      setIsSubmitting(false)
      return
    }

    const quizRequest: CreateQuizTaskRequest = {
      question: taskForm.question,
      optionA: taskForm.optionA,
      optionB: taskForm.optionB,
      optionC: taskForm.optionC,
      optionD: taskForm.optionD,
      correctAnswer: taskForm.correctAnswer,
    }
    const qrRequest: CreateQrCodeTaskRequest = {
      instructions: taskForm.instructions,
      timeLimitMinutes: taskForm.timeLimitMinutes,
      qrPayload: taskForm.qrPayload,
    }
    const gpsRequest: CreateGpsTaskRequest = {
      instructions: taskForm.instructions,
      targetLatitude: taskForm.targetLatitude,
      targetLongitude: taskForm.targetLongitude,
      gameAreaJson: taskForm.gameAreaJson,
      timeLimitMinutes: taskForm.timeLimitMinutes,
    }

    try {
      if (taskForm.id) {
        if (taskForm.taskType === 0) {
          await updateQuizTask(taskForm.id, quizRequest)
        } else if (taskForm.taskType === 1) {
          await updateQrTask(taskForm.id, qrRequest)
        } else {
          await updateGpsTask(taskForm.id, gpsRequest)
        }
      } else if (taskForm.taskType === 0) {
        await createQuizTask(gameId, quizRequest)
      } else if (taskForm.taskType === 1) {
        await createQrTask(gameId, qrRequest)
      } else {
        await createGpsTask(gameId, gpsRequest)
      }

      setTaskDialogOpen(false)
      await refreshGame()
      showSuccess('Görev kaydedildi.')
    } catch (error) {
      setTaskDialogError(getApiErrorMessage(error))
    } finally {
      setIsSubmitting(false)
    }
  }

  const handleDeleteTask = async (taskId: string) => {
    setErrorMessage(null)

    try {
      await deleteTask(taskId)
      await refreshGame()
      showSuccess('Görev silindi.')
    } catch (error) {
      setErrorMessage(getApiErrorMessage(error))
    }
  }

  const handleDropTask = async (targetTaskId: string) => {
    if (!gameId || !draggedTaskId || draggedTaskId === targetTaskId) {
      return
    }

    const taskIds = orderedGörevler.map((task) => task.id)
    const draggedIndex = taskIds.indexOf(draggedTaskId)
    const targetIndex = taskIds.indexOf(targetTaskId)

    taskIds.splice(draggedIndex, 1)
    taskIds.splice(targetIndex, 0, draggedTaskId)

    setErrorMessage(null)

    try {
      const tasks = await reorderTasks(gameId, taskIds)
      setGame((current) => (current ? { ...current, tasks } : current))
      showSuccess('Görev sırası kaydedildi.')
    } catch (error) {
      setErrorMessage(getApiErrorMessage(error))
    } finally {
      setDraggedTaskId(null)
    }
  }

  const buildAiRequest = (): AiGenerationRequest | null => {
    if (!game) {
      return null
    }

    return {
      gradeLevel: game.gradeLevel,
      subject: game.subject,
      topic: game.topic,
      environmentType: game.environmentType,
      expectedStudentCount: game.expectedStudentCount,
      taskCount: aiTaskCount,
    }
  }

  const handleGenerateQuizTasks = async () => {
    const request = buildAiRequest()

    if (!request) {
      return
    }

    setIsSubmitting(true)
    setErrorMessage(null)

    try {
      setGeneratedQuizTasks(await generateQuizTasks(request))
      setGeneratedQrTasks([])
    } catch (error) {
      setErrorMessage(getApiErrorMessage(error))
    } finally {
      setIsSubmitting(false)
    }
  }

  const handleGenerateQrTasks = async () => {
    const request = buildAiRequest()

    if (!request) {
      return
    }

    setIsSubmitting(true)
    setErrorMessage(null)

    try {
      setGeneratedQrTasks(await generateQrCodeTasks(request))
      setGeneratedQuizTasks([])
    } catch (error) {
      setErrorMessage(getApiErrorMessage(error))
    } finally {
      setIsSubmitting(false)
    }
  }

  const handleAddGeneratedQuizTask = async (
    task: GeneratedQuizTask,
    index: number,
  ) => {
    if (!gameId) {
      return
    }

    setErrorMessage(null)

    try {
      await createQuizTask(gameId, task)
      setGeneratedQuizTasks((current) =>
        current.filter((_, currentIndex) => currentIndex !== index),
      )
      await refreshGame()
      showSuccess('AI quiz görevi oyuna eklendi.')
    } catch (error) {
      setErrorMessage(getApiErrorMessage(error))
    }
  }

  const handleAddGeneratedQrTask = async (
    task: GeneratedQrCodeTask,
    index: number,
  ) => {
    if (!gameId) {
      return
    }

    setErrorMessage(null)

    try {
      await createQrTask(gameId, task)
      setGeneratedQrTasks((current) =>
        current.filter((_, currentIndex) => currentIndex !== index),
      )
      await refreshGame()
      showSuccess('AI QR görevi oyuna eklendi.')
    } catch (error) {
      setErrorMessage(getApiErrorMessage(error))
    }
  }

  if (isLoading) {
    return <LoadingState message="Oyun yükleniyor..." />
  }

  if (!game) {
    return <Alert severity="error">{errorMessage ?? 'Oyun bulunamadı.'}</Alert>
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
            {game.subject} / {game.topic}
          </Typography>
          <Stack direction="row" spacing={1} sx={{ alignItems: 'center', mt: 1 }}>
            <StatusChip status={game.status} />
            <Typography color="text.secondary">Oyun kodu: {game.gameCode}</Typography>
          </Stack>
        </Box>
        <Stack direction={{ xs: 'column', sm: 'row' }} spacing={1}>
          <Button component={RouterLink} to={`/games/${game.id}/preview`}>
            Önizleme
          </Button>
          <Button component={RouterLink} to={`/games/${game.id}/results`}>
            Sonuçlar
          </Button>
          {game.status === 2 ? (
            <Button
              color="warning"
              onClick={() =>
                void runLifecycleAction(deactivateGame, 'Oyun pasifleştirildi.')
              }
              variant="outlined"
            >
              Pasifleştir
            </Button>
          ) : (
            <Button
              onClick={() => void runLifecycleAction(activateGame, 'Oyun aktifleştirildi.')}
              variant="contained"
            >
              Aktif et
            </Button>
          )}
        </Stack>
      </Stack>

      {errorMessage && <Alert severity="error">{errorMessage}</Alert>}
      {successMessage && <Alert severity="success">{successMessage}</Alert>}

      <Paper>
        <Tabs value={tab} onChange={(_, value: number) => setTab(value)}>
          <Tab label="Detaylar" />
          <Tab label="Görevler" />
          <Tab label="AI Taslakları" />
        </Tabs>
      </Paper>

      {tab === 0 && (
        <Paper sx={{ p: 3 }}>
          <GameForm
            game={game}
            isSubmitting={isSubmitting}
            submitLabel="Kaydet"
            onSubmit={handleUpdateGame}
          />
        </Paper>
      )}

      {tab === 1 && (
        <Stack spacing={2}>
          <Stack direction={{ xs: 'column', sm: 'row' }} spacing={1}>
            <Button onClick={() => openCreateTaskDialog(0)} variant="contained">
              Quiz ekle
            </Button>
            <Button onClick={() => openCreateTaskDialog(1)} variant="contained">
              QR ekle
            </Button>
            <Button onClick={() => openCreateTaskDialog(2)} variant="contained">
              GPS ekle
            </Button>
          </Stack>

          {orderedGörevler.length === 0 ? (
            <Alert severity="info">Henüz görev yok. Manuel veya AI tarafından üretilmiş bir görev ekleyin.</Alert>
          ) : (
            orderedGörevler.map((task) => (
              <Card
                draggable
                key={task.id}
                onDragOver={(event) => event.preventDefault()}
                onDragStart={() => setDraggedTaskId(task.id)}
                onDrop={() => void handleDropTask(task.id)}
                sx={{ cursor: 'grab' }}
              >
                <CardContent>
                  <Stack spacing={1}>
                    <Typography variant="overline">
                      #{task.order} - {learningTaskTypeLabels[task.taskType]}
                    </Typography>
                    <Typography variant="h6">
                      {task.taskType === 0
                        ? task.question
                        : task.instructions}
                    </Typography>
                    {task.taskType === 0 && (
                      <Typography color="text.secondary">
                        Doğru cevap:{' '}
                        {task.correctAnswer !== null &&
                        task.correctAnswer !== undefined
                          ? quizAnswerOptionLabels[task.correctAnswer]
                          : '-'}
                      </Typography>
                    )}
                    {task.taskType === 1 && (
                      <Typography color="text.secondary">
                        QR içeriği: {task.qrPayload}
                      </Typography>
                    )}
                    {task.taskType === 2 && (
                      <Typography color="text.secondary">
                        Hedef: {task.targetLatitude}, {task.targetLongitude}
                      </Typography>
                    )}
                  </Stack>
                </CardContent>
                <CardActions>
                  <Button onClick={() => openEditTaskDialog(task)}>Düzenle</Button>
                  <Button
                    color="error"
                    onClick={() => void handleDeleteTask(task.id)}
                  >
                    Sil
                  </Button>
                </CardActions>
              </Card>
            ))
          )}
        </Stack>
      )}

      {tab === 2 && (
        <Paper sx={{ p: 3 }}>
          <Stack spacing={3}>
            <Typography variant="h6">AI Taslak Üretimi</Typography>
            <TextField
              label="Görev sayısı"
              type="number"
              value={aiTaskCount}
              onChange={(event) => setAiTaskCount(Number(event.target.value))}
            />
            <Stack direction={{ xs: 'column', sm: 'row' }} spacing={1}>
              <Button
                disabled={isSubmitting}
                onClick={() => void handleGenerateQuizTasks()}
                variant="contained"
              >
                Quiz görevleri üret
              </Button>
              <Button
                disabled={isSubmitting}
                onClick={() => void handleGenerateQrTasks()}
                variant="contained"
              >
                QR görevleri üret
              </Button>
            </Stack>
            {generatedQuizTasks.map((task, index) => (
              <Card key={`${task.question}-${index}`}>
                <CardContent>
                  <Typography variant="h6">{task.question}</Typography>
                  <Typography color="text.secondary">
                    A: {task.optionA} - B: {task.optionB} - C: {task.optionC} - D:{' '}
                    {task.optionD}
                  </Typography>
                </CardContent>
                <CardActions>
                  <Button onClick={() => void handleAddGeneratedQuizTask(task, index)}>
                    Oyuna ekle
                  </Button>
                </CardActions>
              </Card>
            ))}
            {generatedQrTasks.map((task, index) => (
              <Card key={`${task.instructions}-${index}`}>
                <CardContent>
                  <Typography variant="h6">{task.instructions}</Typography>
                  <Typography color="text.secondary">
                    Süre limiti: {task.timeLimitMinutes} dakika
                  </Typography>
                  <Typography color="text.secondary">
                    QR kelimesi: {task.qrPayload}
                  </Typography>
                </CardContent>
                <CardActions>
                  <Button onClick={() => void handleAddGeneratedQrTask(task, index)}>
                    Oyuna ekle
                  </Button>
                </CardActions>
              </Card>
            ))}
          </Stack>
        </Paper>
      )}

      <TaskDialog
        form={taskForm}
        errorMessage={taskDialogError}
        isOpen={taskDialogOpen}
        isSubmitting={isSubmitting}
        onChange={setTaskForm}
        onClose={() => {
          setTaskDialogError(null)
          setTaskDialogOpen(false)
        }}
        onSubmit={handleTaskSubmit}
      />
    </Stack>
  )
}

function TaskDialog({
  errorMessage,
  form,
  isOpen,
  isSubmitting,
  onChange,
  onClose,
  onSubmit,
}: {
  errorMessage: string | null
  form: TaskFormState
  isOpen: boolean
  isSubmitting: boolean
  onChange: (form: TaskFormState) => void
  onClose: () => void
  onSubmit: () => Promise<void>
}) {
  const [isQrPreviewVisible, setIsQrPreviewVisible] = useState(false)
  const update = <Key extends keyof TaskFormState>(
    key: Key,
    value: TaskFormState[Key],
  ) => onChange({ ...form, [key]: value })

  return (
    <Dialog fullWidth maxWidth="md" open={isOpen} onClose={onClose}>
      <DialogTitle>{form.id ? 'Görevi düzenle' : 'Görev ekle'}</DialogTitle>
      <DialogContent>
        <Stack spacing={3} sx={{ pt: 1 }}>
          {errorMessage && <Alert severity="error">{errorMessage}</Alert>}

          <FormControl disabled={Boolean(form.id)}>
            <InputLabel id="task-type-label">Görev tipi</InputLabel>
            <Select
              label="Görev tipi"
              labelId="task-type-label"
              value={form.taskType}
              onChange={(event) =>
                update('taskType', Number(event.target.value) as LearningTaskType)
              }
            >
              <MenuItem value={0}>Quiz</MenuItem>
              <MenuItem value={1}>QR Kod</MenuItem>
              <MenuItem value={2}>GPS</MenuItem>
            </Select>
          </FormControl>

          {form.taskType === 0 && (
            <>
              <TextField
                label="Soru"
                value={form.question}
                onChange={(event) => update('question', event.target.value)}
              />
              <TextField
                label="A seçeneği"
                value={form.optionA}
                onChange={(event) => update('optionA', event.target.value)}
              />
              <TextField
                label="B seçeneği"
                value={form.optionB}
                onChange={(event) => update('optionB', event.target.value)}
              />
              <TextField
                label="C seçeneği"
                value={form.optionC}
                onChange={(event) => update('optionC', event.target.value)}
              />
              <TextField
                label="D seçeneği"
                value={form.optionD}
                onChange={(event) => update('optionD', event.target.value)}
              />
              <FormControl>
                <InputLabel id="correct-answer-label">Doğru cevap</InputLabel>
                <Select
                  label="Doğru cevap"
                  labelId="correct-answer-label"
                  value={form.correctAnswer}
                  onChange={(event) =>
                    update(
                      'correctAnswer',
                      Number(event.target.value) as QuizAnswerOption,
                    )
                  }
                >
                  <MenuItem value={0}>A</MenuItem>
                  <MenuItem value={1}>B</MenuItem>
                  <MenuItem value={2}>C</MenuItem>
                  <MenuItem value={3}>D</MenuItem>
                </Select>
              </FormControl>
            </>
          )}

          {(form.taskType === 1 || form.taskType === 2) && (
            <>
              <TextField
                label="Yönergeler"
                multiline
                minRows={3}
                value={form.instructions}
                onChange={(event) => update('instructions', event.target.value)}
              />
              <TextField
                label="Süre limiti (dakika)"
                type="number"
                value={form.timeLimitMinutes}
                onChange={(event) =>
                  update('timeLimitMinutes', Number(event.target.value))
                }
              />
            </>
          )}

          {form.taskType === 1 && (
            <>
              <TextField
                helperText="Öğrencinin tarayacağı QR kod bu kelimeyi içerecek."
                label="QR kelimesi"
                value={form.qrPayload}
                onChange={(event) => {
                  update('qrPayload', event.target.value)
                  setIsQrPreviewVisible(false)
                }}
              />
              <Stack direction={{ xs: 'column', sm: 'row' }} spacing={1}>
                <Button
                  disabled={!form.qrPayload.trim()}
                  onClick={() => setIsQrPreviewVisible(true)}
                  variant="outlined"
                >
                  QR Kod oluştur
                </Button>
                <Button
                  disabled={!form.qrPayload.trim() || !isQrPreviewVisible}
                  onClick={() => printQrCode(form.qrPayload)}
                  variant="outlined"
                >
                  Yazdır
                </Button>
              </Stack>
              {isQrPreviewVisible && form.qrPayload.trim() && (
                <Stack spacing={1} sx={{ alignItems: 'flex-start' }}>
                  <Box
                    sx={{
                      bgcolor: 'white',
                      border: '1px solid',
                      borderColor: 'divider',
                      borderRadius: 2,
                      p: 2,
                    }}
                  >
                    <QRCodeCanvas
                      id="qr-code-preview"
                      size={220}
                      value={form.qrPayload.trim()}
                    />
                  </Box>
                  <Typography color="text.secondary">
                    QR içeriği: {form.qrPayload.trim()}
                  </Typography>
                </Stack>
              )}
            </>
          )}

          {form.taskType === 2 && (
            <>
              <Box sx={{ height: 320, overflow: 'hidden', borderRadius: 2 }}>
                <MapContainer
                  center={[form.targetLatitude, form.targetLongitude]}
                  scrollWheelZoom
                  style={{ height: '100%', width: '100%' }}
                  zoom={15}
                >
                  <MapSearchControl
                    onLocationFound={(latitude, longitude) =>
                      onChange({
                        ...form,
                        targetLatitude: latitude,
                        targetLongitude: longitude,
                      })
                    }
                  />
                  <TileLayer
                    attribution="&copy; OpenStreetMap contributors"
                    url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
                  />
                  <TaskLocationPicker
                    latitude={form.targetLatitude}
                    longitude={form.targetLongitude}
                    onChange={(latitude, longitude) =>
                      onChange({
                        ...form,
                        targetLatitude: latitude,
                        targetLongitude: longitude,
                      })
                    }
                  />
                </MapContainer>
              </Box>
              <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
                <TextField
                  fullWidth
                  label="Hedef enlem"
                  type="number"
                  value={form.targetLatitude}
                  onChange={(event) =>
                    update('targetLatitude', Number(event.target.value))
                  }
                />
                <TextField
                  fullWidth
                  label="Hedef boylam"
                  type="number"
                  value={form.targetLongitude}
                  onChange={(event) =>
                    update('targetLongitude', Number(event.target.value))
                  }
                />
              </Stack>
              <TextField
                helperText="Backend doğrulamasında kullanılan GeoJSON Polygon metni."
                label="Oyun alanı GeoJSON"
                multiline
                minRows={6}
                value={form.gameAreaJson}
                onChange={(event) => update('gameAreaJson', event.target.value)}
              />
            </>
          )}

          <Divider />
          <Stack
            direction="row"
            spacing={1}
            sx={{ justifyContent: 'flex-end' }}
          >
            <Button disabled={isSubmitting} onClick={onClose}>
              İptal
            </Button>
            <Button
              disabled={isSubmitting}
              onClick={() => void onSubmit()}
              variant="contained"
            >
              Görevi kaydet
            </Button>
          </Stack>
        </Stack>
      </DialogContent>
    </Dialog>
  )
}

function printQrCode(qrPayload: string) {
  const canvas = document.getElementById('qr-code-preview') as HTMLCanvasElement | null
  const imageUrl = canvas?.toDataURL('image/png')

  if (!imageUrl) {
    return
  }

  const printWindow = window.open('', '_blank', 'width=480,height=640')

  if (!printWindow) {
    return
  }

  printWindow.document.write(`
    <!doctype html>
    <html>
      <head>
        <title>QR Kod</title>
        <style>
          body {
            align-items: center;
            display: flex;
            flex-direction: column;
            font-family: Arial, sans-serif;
            gap: 16px;
            justify-content: center;
            min-height: 100vh;
          }
          img {
            height: 320px;
            width: 320px;
          }
          .payload {
            font-size: 24px;
            font-weight: 700;
          }
        </style>
      </head>
      <body>
        <img alt="QR Kod" src="${imageUrl}" />
        <div class="payload">${escapeHtml(qrPayload)}</div>
        <script>
          window.onload = function () {
            window.print();
          };
        </script>
      </body>
    </html>
  `)
  printWindow.document.close()
}

function escapeHtml(value: string) {
  return value
    .replaceAll('&', '&amp;')
    .replaceAll('<', '&lt;')
    .replaceAll('>', '&gt;')
    .replaceAll('"', '&quot;')
    .replaceAll("'", '&#039;')
}



