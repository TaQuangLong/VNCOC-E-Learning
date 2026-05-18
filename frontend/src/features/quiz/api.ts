import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { apiClient } from '@/lib/api-client'
import { progressKeys } from '@/features/progress/api'
import type {
  Quiz,
  CreateQuizRequest,
  CreateQuizResponse,
  UpdateQuizRequest,
  UpdateQuizResponse,
  QuestionUpsertRequest,
  QuestionUpsertResponse,
  SubmitQuizRequest,
  SubmitQuizResponse,
  MyQuizAttemptsResponse,
} from './types'

export const quizKeys = {
  lessonQuiz: (lessonId: number) => ['quiz', 'lesson', lessonId] as const,
  myAttempts: (quizId: number) => ['quiz', 'attempts', 'me', quizId] as const,
}

// GET /api/lessons/{lessonId}/quiz — used by both admin and student
export function useLessonQuiz(lessonId: number) {
  return useQuery({
    queryKey: quizKeys.lessonQuiz(lessonId),
    queryFn: () =>
      apiClient.get<Quiz>(`/lessons/${lessonId}/quiz`).then((r) => r.data),
    enabled: lessonId > 0,
    retry: false, // 404 = no quiz yet — don't retry
  })
}

// POST /api/admin/lessons/{lessonId}/quiz
export function useCreateQuiz(lessonId: number) {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (data: CreateQuizRequest) =>
      apiClient
        .post<CreateQuizResponse>(`/admin/lessons/${lessonId}/quiz`, data)
        .then((r) => r.data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: quizKeys.lessonQuiz(lessonId) })
    },
  })
}

// PUT /api/admin/quizzes/{quizId}
export function useUpdateQuiz(lessonId: number) {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({
      quizId,
      data,
    }: {
      quizId: number
      data: UpdateQuizRequest
    }) =>
      apiClient
        .put<UpdateQuizResponse>(`/admin/quizzes/${quizId}`, data)
        .then((r) => r.data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: quizKeys.lessonQuiz(lessonId) })
    },
  })
}

// POST /api/admin/quizzes/{quizId}/questions
export function useAddQuestion(lessonId: number) {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({
      quizId,
      data,
    }: {
      quizId: number
      data: QuestionUpsertRequest
    }) =>
      apiClient
        .post<QuestionUpsertResponse>(`/admin/quizzes/${quizId}/questions`, data)
        .then((r) => r.data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: quizKeys.lessonQuiz(lessonId) })
    },
  })
}

// PUT /api/admin/questions/{questionId}
export function useUpdateQuestion(lessonId: number) {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({
      questionId,
      data,
    }: {
      questionId: number
      data: QuestionUpsertRequest
    }) =>
      apiClient
        .put<QuestionUpsertResponse>(`/admin/questions/${questionId}`, data)
        .then((r) => r.data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: quizKeys.lessonQuiz(lessonId) })
    },
  })
}

// DELETE /api/admin/questions/{questionId}
export function useDeleteQuestion(lessonId: number) {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (questionId: number) =>
      apiClient.delete(`/admin/questions/${questionId}`).then((r) => r.data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: quizKeys.lessonQuiz(lessonId) })
    },
  })
}

// POST /api/quizzes/{quizId}/submit
export function useSubmitQuiz(courseId: number) {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({
      quizId,
      data,
    }: {
      quizId: number
      data: SubmitQuizRequest
    }) =>
      apiClient
        .post<SubmitQuizResponse>(`/quizzes/${quizId}/submit`, data)
        .then((r) => r.data),
    onSuccess: (_result, { quizId }) => {
      qc.invalidateQueries({ queryKey: quizKeys.myAttempts(quizId) })
      qc.invalidateQueries({ queryKey: progressKeys.course(courseId) })
    },
  })
}

// GET /api/quizzes/{quizId}/attempts/me
export function useMyQuizAttempts(quizId: number) {
  return useQuery({
    queryKey: quizKeys.myAttempts(quizId),
    queryFn: () =>
      apiClient
        .get<MyQuizAttemptsResponse>(`/quizzes/${quizId}/attempts/me`)
        .then((r) => r.data),
    enabled: quizId > 0,
  })
}
