import { useEffect, useState } from 'react'
import { Link as RouterLink, useParams } from 'react-router-dom'
import {
  Box,
  Button,
  Card,
  CardContent,
  Grid,
  Paper,
  Stack,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  Typography,
} from '@mui/material'
import { ErrorState, LoadingState } from '../../components/PageState'
import type { GameResults } from '../../types/api'
import { getGameResults } from '../../services/results/resultsApi'
import { getApiErrorMessage } from '../../utils/apiError'
import { sessionEndReasonLabels } from '../../utils/enumLabels'

export function ResultsPage() {
  const { gameId } = useParams()
  const [results, setResults] = useState<GameResults | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [errorMessage, setErrorMessage] = useState<string | null>(null)

  useEffect(() => {
    const loadSonuçlar = async () => {
      if (!gameId) {
        return
      }

      setIsLoading(true)
      setErrorMessage(null)

      try {
        setResults(await getGameResults(gameId))
      } catch (error) {
        setErrorMessage(getApiErrorMessage(error))
      } finally {
        setIsLoading(false)
      }
    }

    void loadSonuçlar()
  }, [gameId])

  if (isLoading) {
    return <LoadingState message="Sonuçlar yükleniyor..." />
  }

  if (!results) {
    return <ErrorState message={errorMessage ?? 'Sonuçlar bulunamadı.'} />
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
            Sonuçlar Dashboard
          </Typography>
          <Typography color="text.secondary">
            Bu öğrenme oyunu için katılım, tamamlama ve sıralama bilgileri.
          </Typography>
        </Box>
        <Button component={RouterLink} to={`/games/${results.learningGameId}`}>
          Editöre dön
        </Button>
      </Stack>

      <Grid container spacing={2}>
        <Grid size={{ xs: 12, md: 4 }}>
          <SummaryCard label="Beklenen öğrenci" value={results.expectedStudents} />
        </Grid>
        <Grid size={{ xs: 12, md: 4 }}>
          <SummaryCard label="Katılan öğrenci" value={results.joinedStudents} />
        </Grid>
        <Grid size={{ xs: 12, md: 4 }}>
          <SummaryCard label="Tamamlayan öğrenci" value={results.completedStudents} />
        </Grid>
      </Grid>

      <Paper sx={{ overflowX: 'auto' }}>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell>Sıra</TableCell>
              <TableCell>Öğrenci</TableCell>
              <TableCell>Puan</TableCell>
              <TableCell>Durum</TableCell>
              <TableCell>Tamamlanan</TableCell>
              <TableCell>Bitmemiş</TableCell>
              <TableCell>Süresi dolan</TableCell>
              <TableCell>Tamamlama süresi</TableCell>
              <TableCell>Oynanma zamanı</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {results.studentResults.map((student) => (
              <TableRow key={student.studentSessionId}>
                <TableCell>{student.rank}</TableCell>
                <TableCell>{student.studentName}</TableCell>
                <TableCell>{student.totalScore}</TableCell>
                <TableCell>
                  {student.endReason !== null && student.endReason !== undefined
                    ? sessionEndReasonLabels[student.endReason]
                    : 'Devam ediyor'}
                </TableCell>
                <TableCell>{student.completedTaskCount}</TableCell>
                <TableCell>{student.unfinishedTaskCount}</TableCell>
                <TableCell>{student.timedOutTaskCount}</TableCell>
                <TableCell>{student.completionTime}</TableCell>
                <TableCell>{new Date(student.playedAt).toLocaleString()}</TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </Paper>
    </Stack>
  )
}

function SummaryCard({ label, value }: { label: string; value: number }) {
  return (
    <Card>
      <CardContent>
        <Typography color="text.secondary">{label}</Typography>
        <Typography variant="h4">{value}</Typography>
      </CardContent>
    </Card>
  )
}




