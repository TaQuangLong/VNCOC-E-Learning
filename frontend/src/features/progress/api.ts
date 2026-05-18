import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { apiClient } from '@/lib/api-client'
import { enrollmentKeys } from '@/features/enrollment/api'
import type {
  CourseProgress,
  MarkLessonCompleteResponse,
  SaveVideoProgressRequest,
} from './types'

export const progressKeys = {
  course: (courseId: number) => ['progress', 'course', courseId] as const,
}

export function useCourseProgress(courseId: number) {
  return useQuery({
    queryKey: progressKeys.course(courseId),
    queryFn: () =>
      apiClient
        .get<CourseProgress>(`/me/courses/${courseId}/progress`)
        .then((r) => r.data),
    enabled: courseId > 0,
  })
}

export function useMarkLessonComplete(courseId: number) {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (lessonId: number) =>
      apiClient
        .post<MarkLessonCompleteResponse>(`/lessons/${lessonId}/complete`)
        .then((r) => r.data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: progressKeys.course(courseId) })
      qc.invalidateQueries({ queryKey: enrollmentKeys.myEnrolledCourses() })
    },
  })
}

export function useSaveVideoProgress(courseId: number) {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({
      lessonId,
      data,
    }: {
      lessonId: number
      data: SaveVideoProgressRequest
    }) =>
      apiClient
        .post(`/lessons/${lessonId}/video-progress`, data)
        .then((r) => r.data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: progressKeys.course(courseId) })
    },
  })
}
