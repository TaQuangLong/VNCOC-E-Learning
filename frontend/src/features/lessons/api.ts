import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { apiClient } from '@/lib/api-client'
import type {
  LessonSummary,
  LessonDetail,
  Resource,
  LessonFormInput,
  ResourceFormInput,
} from './types'

// --- Query keys ---
export const lessonKeys = {
  all: ['lessons'] as const,
  courseLessons: (courseId: number) =>
    ['lessons', 'course', courseId] as const,
  detail: (id: number) => ['lessons', 'detail', id] as const,
  adminResources: (lessonId: number) =>
    ['lessons', 'admin', 'resources', lessonId] as const,
}

// --- Public / Student ---

export function useCourseLessons(courseId: number) {
  return useQuery({
    queryKey: lessonKeys.courseLessons(courseId),
    queryFn: () =>
      apiClient
        .get<LessonSummary[]>(`/courses/${courseId}/lessons`)
        .then((r) => r.data),
    enabled: courseId > 0,
  })
}

export function useLessonDetail(id: number) {
  return useQuery({
    queryKey: lessonKeys.detail(id),
    queryFn: () =>
      apiClient.get<LessonDetail>(`/lessons/${id}`).then((r) => r.data),
    enabled: id > 0,
  })
}

// --- Admin ---

export function useAdminLessonResources(lessonId: number) {
  return useQuery({
    queryKey: lessonKeys.adminResources(lessonId),
    queryFn: () =>
      apiClient
        .get<Resource[]>(`/admin/lessons/${lessonId}/resources`)
        .then((r) => r.data),
    enabled: lessonId > 0,
  })
}

export function useCreateLesson(courseId: number) {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (data: LessonFormInput) =>
      apiClient
        .post(`/admin/courses/${courseId}/lessons`, data)
        .then((r) => r.data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: lessonKeys.courseLessons(courseId) })
    },
  })
}

export function useUpdateLesson(id: number, courseId: number) {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (data: LessonFormInput) =>
      apiClient.put(`/admin/lessons/${id}`, data).then((r) => r.data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: lessonKeys.detail(id) })
      qc.invalidateQueries({ queryKey: lessonKeys.courseLessons(courseId) })
    },
  })
}

export function useDeleteLesson(courseId: number) {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (lessonId: number) =>
      apiClient.delete(`/admin/lessons/${lessonId}`).then((r) => r.data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: lessonKeys.courseLessons(courseId) })
    },
  })
}

export function useReorderLessons(courseId: number) {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (lessonIds: number[]) =>
      apiClient
        .put(`/admin/courses/${courseId}/lessons/order`, { lessonIds })
        .then((r) => r.data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: lessonKeys.courseLessons(courseId) })
    },
  })
}

export function useAddResource(lessonId: number) {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (data: ResourceFormInput) =>
      apiClient
        .post(`/admin/lessons/${lessonId}/resources`, data)
        .then((r) => r.data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: lessonKeys.adminResources(lessonId) })
    },
  })
}

export function useDeleteResource(lessonId: number) {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (resourceId: number) =>
      apiClient
        .delete(`/admin/resources/${resourceId}`)
        .then((r) => r.data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: lessonKeys.adminResources(lessonId) })
    },
  })
}
