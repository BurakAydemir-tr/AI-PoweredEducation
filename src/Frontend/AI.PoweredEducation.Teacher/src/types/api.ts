export type GameEnvironmentType = 0 | 1

export type LearningGameStatus = 0 | 1 | 2 | 3

export type LearningTaskType = 0 | 1 | 2

export type QuizAnswerOption = 0 | 1 | 2 | 3

export type SessionEndReason = 0 | 1

export type LearningTask = {
  id: string
  taskType: LearningTaskType
  order: number
  question?: string | null
  optionA?: string | null
  optionB?: string | null
  optionC?: string | null
  optionD?: string | null
  correctAnswer?: QuizAnswerOption | null
  instructions?: string | null
  qrPayload?: string | null
  targetLatitude?: number | null
  targetLongitude?: number | null
  gameAreaJson?: string | null
  timeLimitMinutes?: number | null
}

export type LearningGame = {
  id: string
  gradeLevel: string
  subject: string
  topic: string
  environmentType: GameEnvironmentType
  expectedStudentCount: number
  status: LearningGameStatus
  gameCode: string
  tasks: LearningTask[]
}

export type LearningGameRequest = {
  gradeLevel: string
  subject: string
  topic: string
  environmentType: GameEnvironmentType
  expectedStudentCount: number
}

export type CreateQuizTaskRequest = {
  question: string
  optionA: string
  optionB: string
  optionC: string
  optionD: string
  correctAnswer: QuizAnswerOption
}

export type CreateQrCodeTaskRequest = {
  instructions: string
  timeLimitMinutes: number
  qrPayload?: string | null
}

export type CreateGpsTaskRequest = {
  instructions: string
  targetLatitude: number
  targetLongitude: number
  gameAreaJson: string
  timeLimitMinutes: number
}

export type AiGenerationRequest = LearningGameRequest & {
  taskCount: number
}

export type GeneratedQuizTask = CreateQuizTaskRequest

export type GeneratedQrCodeTask = CreateQrCodeTaskRequest

export type GameResults = {
  learningGameId: string
  expectedStudents: number
  joinedStudents: number
  completedStudents: number
  studentResults: StudentResult[]
}

export type StudentResult = {
  rank: number
  studentSessionId: string
  studentName: string
  endReason?: SessionEndReason | null
  totalScore: number
  completedTaskCount: number
  unfinishedTaskCount: number
  timedOutTaskCount: number
  completionTime: string
  playedAt: string
}
