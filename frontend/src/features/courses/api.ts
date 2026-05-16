import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { apiClient } from '@/lib/api-client'
import type {
  AdminCourseDetail,
  AdminCourseSummary,
  AuthorSummary,
  CourseDetail,
  CourseSummary,
  CourseFormInput,
  PaginatedResponse,
} from './types'

// --- Query keys ---
export const courseKeys = {
  all: ['courses'] as const,
  published: (params: object) => ['courses', 'published', params] as const,
  detail: (slug: string) => ['courses', 'detail', slug] as const,
  adminAll: (params: object) => ['courses', 'admin', 'list', params] as const,
  adminDetail: (id: number) => ['courses', 'admin', id] as const,
  authors: () => ['authors'] as const,
}

// --- Public ---

export function usePublishedCourses(params: {
  page?: number
  pageSize?: number
  category?: string
  level?: string
  title?: string
}) {
  return useQuery({
    queryKey: courseKeys.published(params),
    queryFn: () =>
      apiClient
        .get<PaginatedResponse<CourseSummary>>('/courses', { params })
        .then((r) => r.data),
  })
}

export function useCourseBySlug(slug: string) {
  return useQuery({
    queryKey: courseKeys.detail(slug),
    queryFn: () =>
      apiClient.get<CourseDetail>(`/courses/${slug}`).then((r) => r.data),
    enabled: !!slug,
  })
}

// --- Admin ---

export function useAdminCourses(params: {
  page?: number
  pageSize?: number
  status?: string
  title?: string
}) {
  return useQuery({
    queryKey: courseKeys.adminAll(params),
    queryFn: () =>
      apiClient
        .get<PaginatedResponse<AdminCourseSummary>>('/admin/courses', { params })
        .then((r) => r.data),
  })
}

export function useAdminCourse(id: number) {
  return useQuery({
    queryKey: courseKeys.adminDetail(id),
    queryFn: () =>
      apiClient
        .get<AdminCourseDetail>(`/admin/courses/${id}`)
        .then((r) => r.data),
    enabled: id > 0,
  })
}

export function useAuthors() {
  return useQuery({
    queryKey: courseKeys.authors(),
    queryFn: () =>
      apiClient.get<AuthorSummary[]>('/admin/authors').then((r) => r.data),
  })
}

// --- Mutations ---

export function useCreateCourse() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (data: CourseFormInput) =>
      apiClient.post('/admin/courses', data).then((r) => r.data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: courseKeys.all })
    },
  })
}

export function useUpdateCourse(id: number) {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (data: CourseFormInput) =>
      apiClient.put(`/admin/courses/${id}`, data).then((r) => r.data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: courseKeys.all })
    },
  })
}

export function useDeleteCourse() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (id: number) =>
      apiClient.delete(`/admin/courses/${id}`).then((r) => r.data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: courseKeys.all })
    },
  })
}

export function usePublishCourse() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (id: number) =>
      apiClient.post(`/admin/courses/${id}/publish`).then((r) => r.data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: courseKeys.all })
    },
  })
}

export function useUnpublishCourse() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (id: number) =>
      apiClient.post(`/admin/courses/${id}/unpublish`).then((r) => r.data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: courseKeys.all })
    },
  })
}
