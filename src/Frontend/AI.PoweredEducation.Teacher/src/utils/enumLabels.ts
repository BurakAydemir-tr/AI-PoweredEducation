import type {
  GameEnvironmentType,
  LearningGameStatus,
  LearningTaskType,
  QuizAnswerOption,
  SessionEndReason,
} from '../types/api'

export const gameEnvironmentTypeLabels: Record<GameEnvironmentType, string> = {
  0: 'İç mekan',
  1: 'Dış mekan',
}

export const learningGameStatusLabels: Record<LearningGameStatus, string> = {
  0: 'Taslak',
  1: 'Pasif',
  2: 'Aktif',
  3: 'Arşivlendi',
}

export const learningTaskTypeLabels: Record<LearningTaskType, string> = {
  0: 'Quiz',
  1: 'QR Kod',
  2: 'GPS',
}

export const quizAnswerOptionLabels: Record<QuizAnswerOption, string> = {
  0: 'A',
  1: 'B',
  2: 'C',
  3: 'D',
}

export const sessionEndReasonLabels: Record<SessionEndReason, string> = {
  0: 'Tamamlandı',
  1: 'Ayrıldı',
}

export const defaultGameAreaJson = JSON.stringify(
  {
    type: 'Polygon',
    coordinates: [
      [
        [32.8541, 39.9208],
        [32.8561, 39.9208],
        [32.8561, 39.9228],
        [32.8541, 39.9228],
        [32.8541, 39.9208],
      ],
    ],
  },
  null,
  2,
)
