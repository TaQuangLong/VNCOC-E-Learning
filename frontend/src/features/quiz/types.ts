import { z } from 'zod'

// --- Question type enum ---
export type QuestionType = 'SingleChoice' | 'MultipleChoice' | 'TrueFalse'

// --- Answer option (student-facing, no IsCorrect) ---
export interface AnswerOption {
  id: number
  text: string
  orderIndex: number
}

// --- Question (student-facing) ---
export interface Question {
  id: number
  text: string
  type: QuestionType
  orderIndex: number
  options: AnswerOption[]
}

// --- Quiz (GET /api/lessons/{lessonId}/quiz) ---
export interface Quiz {
  id: number
  title: string
  description: string | null
  passingScore: number
  isRequired: boolean
  questions: Question[]
}

// ─── Admin types ───────────────────────────────────────────────────────────────

export interface CreateAnswerOptionDto {
  text: string
  isCorrect: boolean
  orderIndex: number
}

export interface CreateQuestionDto {
  text: string
  type: QuestionType
  orderIndex: number
  options: CreateAnswerOptionDto[]
}

export interface CreateQuizRequest {
  title: string
  description?: string | null
  passingScore: number
  isRequired: boolean
  questions: CreateQuestionDto[]
}

export interface CreateQuizResponse {
  id: number
  title: string
  questionCount: number
}

export interface UpdateQuizRequest {
  title: string
  description?: string | null
  passingScore: number
  isRequired: boolean
}

export interface UpdateQuizResponse {
  id: number
  title: string
  passingScore: number
  isRequired: boolean
}

export interface QuestionOptionDto {
  text: string
  isCorrect: boolean
  orderIndex: number
}

export interface QuestionUpsertRequest {
  text: string
  type: QuestionType
  orderIndex: number
  options: QuestionOptionDto[]
}

export interface QuestionUpsertResponse {
  id: number
  text: string
  type: string
  orderIndex: number
}

// ─── Student types ──────────────────────────────────────────────────────────────

export interface SubmitAnswerDto {
  questionId: number
  selectedOptionIds: number[]
}

export interface SubmitQuizRequest {
  answers: SubmitAnswerDto[]
}

export interface AnswerResultDto {
  questionId: number
  isCorrect: boolean
  correctOptionIds: number[]
  selectedOptionIds: number[]
}

export interface SubmitQuizResponse {
  attemptId: number
  score: number
  passed: boolean
  correctCount: number
  totalCount: number
  results: AnswerResultDto[]
}

export interface QuizAttemptSummary {
  id: number
  score: number
  passed: boolean
  submittedAt: string
}

export interface MyQuizAttemptsResponse {
  quizId: number
  attempts: QuizAttemptSummary[]
  bestScore: number | null
}

// ─── Zod schemas for admin forms ────────────────────────────────────────────────

export const answerOptionFormSchema = z.object({
  text: z.string().min(1, 'Answer text is required').max(500),
  isCorrect: z.boolean(),
  orderIndex: z.number(),
})

export const questionFormSchema = z.object({
  text: z.string().min(1, 'Question text is required').max(1000),
  type: z.enum(['SingleChoice', 'MultipleChoice', 'TrueFalse']),
  orderIndex: z.number(),
  options: z
    .array(answerOptionFormSchema)
    .min(2, 'At least 2 options required'),
})

export type AnswerOptionFormInput = z.infer<typeof answerOptionFormSchema>
export type QuestionFormInput = z.infer<typeof questionFormSchema>

export const quizMetaFormSchema = z.object({
  title: z.string().min(1, 'Title is required').max(200),
  description: z.string().max(1000).optional(),
  passingScore: z.number().min(0, 'Min 0').max(100, 'Max 100'),
  isRequired: z.boolean(),
})

export type QuizMetaFormInput = z.infer<typeof quizMetaFormSchema>
